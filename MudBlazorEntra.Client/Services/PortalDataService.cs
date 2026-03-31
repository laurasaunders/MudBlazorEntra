using System.Text.Json;
using System.Net.Http.Json;
using MudBlazorEntra.Client.Models.Authentication;
using MudBlazorEntra.Client.Models.Cases;
using MudBlazorEntra.Client.Models.Policies;
using MudBlazorEntra.Client.Models.Users;
using MudBlazorEntra.Client.Models.WhiteLabel;

namespace MudBlazorEntra.Client.Services;

public class PortalDataService(HttpClient httpClient)
{
    private static readonly string[] DefaultTestPanels = ["base", "anotherCompany", "anothercompany"];

    public async Task<UserDetailsResponse> GetCurrentUserDetailsAsync()
    {
        try
        {
            var currentUser = await httpClient.GetFromJsonAsync<CurrentUserInfoResponse>("api/account/me");
            if (currentUser is null || string.IsNullOrWhiteSpace(currentUser.UserId))
            {
                throw new InvalidOperationException("Current user details are unavailable.");
            }

            var userDetails = await httpClient.GetFromJsonAsync<UserDetailsResponse>($"api/users/{Uri.EscapeDataString(currentUser.UserId)}/details");
            if (userDetails is null)
            {
                throw new InvalidOperationException("Downstream user details are unavailable.");
            }

            return EnsureTestPanels(userDetails);
        }
        catch (Exception ex) when (ex is HttpRequestException or NotSupportedException or JsonException or InvalidOperationException)
        {
            var currentUser = await TryGetCurrentUserAsync();
            return EnsureTestPanels(CaseSampleDataService.GetFallbackUserDetails(currentUser));
        }
    }

    public async Task<IReadOnlyList<CaseListItem>> GetPoliciesAsync()
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<List<CaseListItem>>("api/policies");
            return result ?? CaseSampleDataService.Cases;
        }
        catch (Exception ex) when (ex is HttpRequestException or NotSupportedException or JsonException)
        {
            return CaseSampleDataService.Cases;
        }
    }

    public async Task<PolicyDetailsResponse?> GetPolicyByIdAsync(string policyId)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<PolicyDetailsResponse>($"api/policies/{Uri.EscapeDataString(policyId)}");
        }
        catch (Exception ex) when (ex is HttpRequestException or NotSupportedException or JsonException)
        {
            return CaseSampleDataService.GetFallbackPolicyDetails(policyId);
        }
    }

    public async Task<string?> GetCurrentSiteVersionAsync()
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<CurrentSiteVersionResponse>("api/site-version/current");
            return string.IsNullOrWhiteSpace(result?.PanelKey) ? null : result.PanelKey;
        }
        catch (Exception ex) when (ex is HttpRequestException or NotSupportedException or JsonException)
        {
            return null;
        }
    }

    private async Task<CurrentUserInfoResponse?> TryGetCurrentUserAsync()
    {
        try
        {
            return await httpClient.GetFromJsonAsync<CurrentUserInfoResponse>("api/account/me");
        }
        catch (Exception ex) when (ex is HttpRequestException or NotSupportedException or JsonException)
        {
            return null;
        }
    }

    private static UserDetailsResponse EnsureTestPanels(UserDetailsResponse userDetails)
    {
        var mergedPanels = userDetails.Panels
            .Concat(DefaultTestPanels)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        userDetails.Panels = mergedPanels;
        return userDetails;
    }
}
