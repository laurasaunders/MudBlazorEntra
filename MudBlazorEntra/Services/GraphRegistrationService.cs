using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MudBlazorEntra.Client.Models.Authentication;
using MudBlazorEntra.Options;

namespace MudBlazorEntra.Services;

public class GraphRegistrationService(
    HttpClient httpClient,
    EntraAccessTokenProvider accessTokenProvider,
    IOptions<GraphRegistrationOptions> options,
    IOptions<GraphApiOptions> graphApiOptions,
    IRegistrationEligibilityService registrationEligibilityService,
    IRegistrationEmailSender registrationEmailSender)
{
    private const string GraphUsersEndpoint = "https://graph.microsoft.com/v1.0/users";
    private const string GraphUserLookupTemplate = "https://graph.microsoft.com/v1.0/users?$select=id&$top=1&$filter=identities/any(c:c/issuerAssignedId eq '{0}' and c/issuer eq '{1}')";

    public async Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.IssuerDomain) || string.IsNullOrWhiteSpace(settings.LoginUrl))
        {
            throw new InvalidOperationException("Graph registration configuration is incomplete. Set IssuerDomain and LoginUrl.");
        }

        var isEligible = await registrationEligibilityService.CanRegisterAsync(request.Email, cancellationToken);
        if (!isEligible)
        {
            throw new InvalidOperationException("The supplied email address is not allowed to register.");
        }

        if (await UserExistsAsync(request.Email, settings.IssuerDomain, cancellationToken))
        {
            throw new RegistrationConflictException("A user with that email address already exists.");
        }

        var temporaryPassword = $"Temp!{Guid.NewGuid():N}";
        var displayName = $"{request.FirstName} {request.LastName}".Trim();
        var accessToken = await accessTokenProvider.GetAccessTokenAsync(
            graphApiOptions.Value.TenantId,
            graphApiOptions.Value.ClientId,
            graphApiOptions.Value.ClientSecret,
            graphApiOptions.Value.Scope,
            cancellationToken);
        using var graphRequest = new HttpRequestMessage(HttpMethod.Post, GraphUsersEndpoint)
        {
            Content = JsonContent.Create(new
            {
                accountEnabled = true,
                displayName,
                mail = request.Email,
                mailNickname = CreateMailNickname(request.Email),
                identities = new[]
                {
                    new
                    {
                        signInType = "emailAddress",
                        issuer = settings.IssuerDomain,
                        issuerAssignedId = request.Email
                    }
                },
                passwordProfile = new
                {
                    password = temporaryPassword,
                    forceChangePasswordNextSignIn = true
                },
                passwordPolicies = "DisablePasswordExpiration",
                givenName = request.FirstName,
                surname = request.LastName
            })
        };

        graphRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(graphRequest, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(BuildFailureMessage(response.StatusCode, payload));
        }

        var createdUserId = ExtractCreatedUserId(payload);
        try
        {
            await registrationEmailSender.SendTemporaryPasswordAsync(
                request.Email,
                displayName,
                temporaryPassword,
                settings.LoginUrl,
                cancellationToken);
        }
        catch (Exception emailException)
        {
            await DeleteUserIfCreatedAsync(createdUserId, accessToken, cancellationToken);
            throw new InvalidOperationException(
                "The user account was created but the welcome email failed, so the account was deleted. Please try again.",
                emailException);
        }

        return new RegisterUserResponse
        {
            Email = request.Email,
            Message = "Registration submitted. The account was created and a temporary password email was sent to the user."
        };
    }

    private static void ValidateRequest(RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new InvalidOperationException("First name, last name, and email address are required.");
        }
    }

    private static string CreateMailNickname(string emailAddress)
    {
        var localPart = emailAddress.Split('@', 2)[0];
        var sanitized = new string(localPart.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "user";
        }

        return $"{sanitized[..Math.Min(20, sanitized.Length)]}{Guid.NewGuid():N}"[..30];
    }

    private static string BuildFailureMessage(HttpStatusCode statusCode, string payload)
    {
        if (statusCode == HttpStatusCode.BadRequest && payload.Contains("ObjectConflict", StringComparison.OrdinalIgnoreCase))
        {
            return "A user with that email address already exists.";
        }

        return $"Microsoft Graph user creation failed with status {(int)statusCode}: {payload}";
    }

    private async Task<bool> UserExistsAsync(string emailAddress, string issuerDomain, CancellationToken cancellationToken)
    {
        var accessToken = await accessTokenProvider.GetAccessTokenAsync(
            graphApiOptions.Value.TenantId,
            graphApiOptions.Value.ClientId,
            graphApiOptions.Value.ClientSecret,
            graphApiOptions.Value.Scope,
            cancellationToken);
        var requestUri = string.Format(
            GraphUserLookupTemplate,
            Uri.EscapeDataString(emailAddress.Replace("'", "''", StringComparison.Ordinal)),
            Uri.EscapeDataString(issuerDomain.Replace("'", "''", StringComparison.Ordinal)));

        using var graphRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
        graphRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(graphRequest, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Microsoft Graph user lookup failed with status {(int)response.StatusCode}: {payload}");
        }

        using var document = JsonDocument.Parse(payload);
        return document.RootElement.TryGetProperty("value", out var valueElement) &&
               valueElement.ValueKind == JsonValueKind.Array &&
               valueElement.GetArrayLength() > 0;
    }

    private static string ExtractCreatedUserId(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        if (document.RootElement.TryGetProperty("id", out var idElement) &&
            idElement.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(idElement.GetString()))
        {
            return idElement.GetString()!;
        }

        throw new InvalidOperationException("Microsoft Graph created the user but did not return an id.");
    }

    private async Task DeleteUserIfCreatedAsync(string userId, string accessToken, CancellationToken cancellationToken)
    {
        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"{GraphUsersEndpoint}/{Uri.EscapeDataString(userId)}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var deleteResponse = await httpClient.SendAsync(deleteRequest, cancellationToken);
        if (!deleteResponse.IsSuccessStatusCode)
        {
            var deletePayload = await deleteResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"The welcome email failed and the compensating delete also failed with status {(int)deleteResponse.StatusCode}: {deletePayload}");
        }
    }
}
