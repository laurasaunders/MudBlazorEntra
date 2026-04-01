namespace MudBlazorEntra.Client.Models.NewEnquiry;

public class RiskOption
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Tag { get; set; }

    public bool IsSelected { get; set; }

    public bool IsDisabled { get; set; }
}
