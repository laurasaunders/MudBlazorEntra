using System.ComponentModel.DataAnnotations;

namespace MudBlazorEntra.Client.Models.Authentication;

public class RegisterUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;
}
