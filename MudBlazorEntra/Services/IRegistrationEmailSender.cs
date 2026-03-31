namespace MudBlazorEntra.Services;

public interface IRegistrationEmailSender
{
    Task SendTemporaryPasswordAsync(string emailAddress, string displayName, string temporaryPassword, string loginUrl, CancellationToken cancellationToken = default);
}
