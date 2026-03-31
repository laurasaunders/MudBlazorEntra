namespace MudBlazorEntra.Options;

public class GraphRegistrationOptions
{
    public const string SectionName = "GraphRegistration";

    public string IssuerDomain { get; set; } = string.Empty;

    public string LoginUrl { get; set; } = string.Empty;
}
