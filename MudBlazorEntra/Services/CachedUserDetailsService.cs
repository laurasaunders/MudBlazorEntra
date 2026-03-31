using Microsoft.Extensions.Caching.Memory;
using MudBlazorEntra.Client.Models.Users;

namespace MudBlazorEntra.Services;

public class CachedUserDetailsService(IMemoryCache memoryCache, FirstTitleOnlineApi firstTitleOnlineApi)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public async Task<UserDetailsResponse> GetUserDetailsAsync(string userId)
    {
        var cacheKey = $"user-details:{userId}";
        if (memoryCache.TryGetValue<UserDetailsResponse>(cacheKey, out var cachedUserDetails) &&
            cachedUserDetails is not null)
        {
            return cachedUserDetails;
        }

        var userDetails = await firstTitleOnlineApi.GetUserDetailsAsync(userId);
        memoryCache.Set(cacheKey, userDetails, CacheDuration);
        return userDetails;
    }
}
