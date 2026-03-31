namespace MudBlazorEntra.Client.Models.Users;

public class UserDetailsResponse
{
    public string UserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string IntroducerId { get; set; } = string.Empty;

    public string OfficeId { get; set; } = string.Empty;

    public string OfficeName { get; set; } = string.Empty;

    public IReadOnlyList<string> Panels { get; set; } = [];
}
