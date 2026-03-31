namespace MudBlazorEntra.Services;

public class RegistrationConflictException(string message) : InvalidOperationException(message);
