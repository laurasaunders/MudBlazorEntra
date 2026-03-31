using Microsoft.AspNetCore.Components;
using MudBlazorEntra.Client.Models.Users;
using MudBlazorEntra.Client.Models.WhiteLabel;
using MudBlazorEntra.Client.Services;

namespace MudBlazorEntra.Client.Pages;

public partial class SiteVersion
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private WhiteLabelContext WhiteLabelContext { get; set; } = default!;

    private UserDetailsResponse? _userDetails;
    private bool _isLoading = true;

    private IReadOnlyList<WhiteLabelPanelOptions> AvailablePanels => WhiteLabelContext.Panels
        .Where(CanAccessPanel)
        .ToList();

    protected override async Task OnInitializedAsync()
    {
        _userDetails = await PortalDataService.GetCurrentUserDetailsAsync();
        _isLoading = false;

        if (AvailablePanels.Count <= 1)
        {
            NavigationManager.NavigateTo("/", forceLoad: true);
        }
    }

    private bool CanAccessPanel(WhiteLabelPanelOptions panel)
    {
        if (_userDetails?.Panels is not { Count: > 0 } panels)
        {
            return string.IsNullOrWhiteSpace(panel.PathPrefix);
        }

        return panels.Any(userPanel =>
            string.Equals(userPanel, panel.Key, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(userPanel, panel.DisplayName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(userPanel, panel.PathPrefix, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetSelectionPath(WhiteLabelPanelOptions panel)
    {
        return $"/site-version/select/{Uri.EscapeDataString(panel.Key)}";
    }
}
