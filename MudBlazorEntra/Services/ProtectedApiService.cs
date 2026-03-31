using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using MudBlazorEntra.Options;

namespace MudBlazorEntra.Services;

public class ProtectedApiService(HttpClient httpClient, EntraAccessTokenProvider accessTokenProvider, IOptions<FirstTitleOnlineApiOptions> options)
{
    private readonly FirstTitleOnlineApiOptions _settings = options.Value;

    public async Task<HttpResponseMessage> GetAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        using var request = await CreateRequestAsync(HttpMethod.Get, relativePath, cancellationToken);
        return await httpClient.SendAsync(request, cancellationToken);
    }

    public async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string relativePath, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var accessToken = await accessTokenProvider.GetAccessTokenAsync(
            _settings.TenantId,
            _settings.ClientId,
            _settings.ClientSecret,
            _settings.Scope,
            cancellationToken);
        var request = new HttpRequestMessage(method, new Uri(new Uri(_settings.BaseUrl), relativePath));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_settings.BaseUrl) ||
            string.IsNullOrWhiteSpace(_settings.TenantId) ||
            string.IsNullOrWhiteSpace(_settings.ClientId) ||
            string.IsNullOrWhiteSpace(_settings.ClientSecret) ||
            string.IsNullOrWhiteSpace(_settings.Scope))
        {
            throw new InvalidOperationException("FirstTitleOnlineApi configuration is incomplete. Set BaseUrl, TenantId, ClientId, ClientSecret, and Scope.");
        }
    }
}
