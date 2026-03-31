namespace MudBlazorEntra.Client.Models.Policies;

public class PolicyDetailsResponse
{
    public string PolicyId { get; set; } = string.Empty;

    public string YourReference { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool IsMine { get; set; }

    public DateTime LastUpdatedDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public string OfficeId { get; set; } = string.Empty;

    public string OfficeName { get; set; } = string.Empty;

    public IReadOnlyList<string> Panels { get; set; } = [];
}
