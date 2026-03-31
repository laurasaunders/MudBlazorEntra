namespace MudBlazorEntra.Client.Models.Cases;

public class CaseListItem
{
    public string YourReference { get; set; } = string.Empty;

    public string CaseId { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool IsMine { get; set; }

    public DateTime LastUpdatedDate { get; set; }

    public DateTime CreatedDate { get; set; }
}
