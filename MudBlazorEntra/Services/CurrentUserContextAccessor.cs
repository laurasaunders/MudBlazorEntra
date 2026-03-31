using System.Security.Claims;

namespace MudBlazorEntra.Services;

public class CurrentUserContextAccessor
{
    public string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirst("oid")?.Value
               ?? user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
               ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? user.FindFirst("sub")?.Value
               ?? string.Empty;
    }
}
