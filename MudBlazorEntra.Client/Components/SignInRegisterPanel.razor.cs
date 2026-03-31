using System.Net.Http.Json;
using System.Text.Json;
using MudBlazor;
using MudBlazorEntra.Client.Models.Authentication;

namespace MudBlazorEntra.Client.Components;

public partial class SignInRegisterPanel
{
    private readonly RegisterUserRequest _registerModel = new();
    private bool _isSubmitting;
    private string? _registrationMessage;
    private string? _accessDeniedMessage;

    protected override void OnParametersSet()
    {
        var absoluteUri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var deniedPanel = GetQueryParameter(absoluteUri.Query, "accessDenied");
        _accessDeniedMessage = string.IsNullOrWhiteSpace(deniedPanel)
            ? null
            : $"You do not have access to the {deniedPanel} site.";
    }

    private async Task SubmitRegistrationAsync()
    {
        _isSubmitting = true;
        _registrationMessage = null;

        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/account/register", _registerModel);
            var payload = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(payload?.Message ?? "Registration failed.");
            }

            _registrationMessage = payload?.Message ?? "Registration submitted.";
            Snackbar.Add(_registrationMessage, Severity.Success);
            _registerModel.FirstName = string.Empty;
            _registerModel.LastName = string.Empty;
            _registerModel.Email = string.Empty;
        }
        catch (Exception ex) when (ex is HttpRequestException or NotSupportedException or JsonException or InvalidOperationException)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private string GetLoginPath()
    {
        var returnUrl = Uri.EscapeDataString(WhiteLabelContext.GetPath("landing"));
        return $"{WhiteLabelContext.RootPath("authentication/login")}?returnUrl={returnUrl}";
    }

    private static string? GetQueryParameter(string queryString, string key)
    {
        if (string.IsNullOrWhiteSpace(queryString))
        {
            return null;
        }

        foreach (var segment in queryString.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = segment.Split('=', 2);
            if (parts.Length == 2 && string.Equals(parts[0], key, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return null;
    }
}
