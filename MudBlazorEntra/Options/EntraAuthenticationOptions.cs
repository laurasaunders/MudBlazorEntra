namespace MudBlazorEntra.Options;

public class EntraAuthenticationOptions
{
    public const string SectionName = "Entra";

    public string Instance { get; set; } = "https://login.microsoftonline.com";

    public string TenantId { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string CallbackPath { get; set; } = "/signin-oidc";

    public string SignedOutCallbackPath { get; set; } = "/signout-callback-oidc";
}
