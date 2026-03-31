using Azure.Core;
using Azure.Identity;
using System.Collections.Concurrent;

namespace MudBlazorEntra.Services;

public class EntraAccessTokenProvider
{
    private static readonly TimeSpan RefreshBuffer = TimeSpan.FromMinutes(10);
    private readonly ConcurrentDictionary<string, CachedAccessToken> _cache = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<string> GetAccessTokenAsync(string tenantId, string clientId, string clientSecret, string scope, CancellationToken cancellationToken = default)
    {
        ValidateCredentials(tenantId, clientId, clientSecret, scope);

        var cacheKey = $"{tenantId}|{clientId}|{scope}";
        if (_cache.TryGetValue(cacheKey, out var cachedToken) &&
            cachedToken.ExpiresOnUtc > DateTimeOffset.UtcNow.Add(RefreshBuffer))
        {
            return cachedToken.Token;
        }

        var scopeLock = _locks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        await scopeLock.WaitAsync(cancellationToken);

        try
        {
            if (_cache.TryGetValue(cacheKey, out cachedToken) &&
                cachedToken.ExpiresOnUtc > DateTimeOffset.UtcNow.Add(RefreshBuffer))
            {
                return cachedToken.Token;
            }

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var token = await credential.GetTokenAsync(new TokenRequestContext([scope]), cancellationToken);
            _cache[cacheKey] = new CachedAccessToken(token.Token, token.ExpiresOn);
            return token.Token;
        }
        finally
        {
            scopeLock.Release();
        }
    }

    private static void ValidateCredentials(string tenantId, string clientId, string clientSecret, string scope)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret) ||
            string.IsNullOrWhiteSpace(scope))
        {
            throw new InvalidOperationException("Client credentials configuration is incomplete. Set TenantId, ClientId, ClientSecret, and Scope.");
        }
    }

    private record CachedAccessToken(string Token, DateTimeOffset ExpiresOnUtc);
}
