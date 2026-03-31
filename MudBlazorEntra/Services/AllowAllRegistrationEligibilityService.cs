namespace MudBlazorEntra.Services;

public class AllowAllRegistrationEligibilityService : IRegistrationEligibilityService
{
    public Task<bool> CanRegisterAsync(string emailAddress, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
