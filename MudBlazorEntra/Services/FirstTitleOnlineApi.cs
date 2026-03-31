using System.Text.Json;
using MudBlazorEntra.Client.Models.Cases;
using MudBlazorEntra.Client.Models.Policies;
using MudBlazorEntra.Client.Models.Users;

namespace MudBlazorEntra.Services;

public class FirstTitleOnlineApi(ProtectedApiService protectedApiService)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<UserDetailsResponse> GetUserDetailsAsync(string userId)
    {
        return await GetAsync<UserDetailsResponse>($"users/{Uri.EscapeDataString(userId)}/details");
    }

    public async Task<IReadOnlyList<CaseListItem>> GetAllPoliciesAsync()
    {
        var result = await GetAsync<List<CaseListItem>>("policies");
        return result;
    }

    public async Task<PolicyDetailsResponse> GetPolicyByIdAsync(string policyId)
    {
        return await GetAsync<PolicyDetailsResponse>($"policies/{Uri.EscapeDataString(policyId)}");
    }

    private async Task<T> GetAsync<T>(string relativePath)
    {
        using var response = await protectedApiService.GetAsync(relativePath);
        var payload = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Downstream FirstTitle Online API call to '{relativePath}' failed with status {(int)response.StatusCode}: {payload}");
        }

        var result = JsonSerializer.Deserialize<T>(payload, JsonOptions);
        if (result is null)
        {
            throw new InvalidOperationException($"Downstream FirstTitle Online API returned an empty payload for '{relativePath}'.");
        }

        return result;
    }
}
