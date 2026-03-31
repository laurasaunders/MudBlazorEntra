namespace MudBlazorEntra.Services;

public interface IRegistrationEligibilityService
{
    Task<bool> CanRegisterAsync(string emailAddress, CancellationToken cancellationToken = default);
}
