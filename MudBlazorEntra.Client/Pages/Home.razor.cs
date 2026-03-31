using Microsoft.AspNetCore.Components;
using MudBlazorEntra.Client.Models.Users;
using MudBlazorEntra.Client.Models.WhiteLabel;
using MudBlazorEntra.Client.Services;

namespace MudBlazorEntra.Client.Pages;

public partial class Home
{
    [Parameter]
    public string? PanelPath { get; set; }
    private UserDetailsResponse? _userDetails;
    private bool _isLoadingPanels = true;
    private string? _selectedPanelKey;
    private bool HasSiteVersionChoices => WhiteLabelContext.Panels.Count(CanAccessPanel) > 1;

    protected override async Task OnInitializedAsync()
    {
        _userDetails = await PortalDataService.GetCurrentUserDetailsAsync();
        _selectedPanelKey = await PortalDataService.GetCurrentSiteVersionAsync();
        _isLoadingPanels = false;

        if (WhiteLabelContext.IsExplicitSelection && !CanAccessPanel(WhiteLabelContext.CurrentPanel))
        {
            var deniedPanel = Uri.EscapeDataString(WhiteLabelContext.CurrentPanel.DisplayName);
            NavigationManager.NavigateTo($"/authentication/logout?returnUrl=%2F%3FaccessDenied%3D{deniedPanel}", forceLoad: true);
            return;
        }

        if (!WhiteLabelContext.IsExplicitSelection && string.IsNullOrWhiteSpace(_selectedPanelKey) && HasSiteVersionChoices)
        {
            NavigationManager.NavigateTo("/site-version", forceLoad: true);
            return;
        }

        if (!WhiteLabelContext.IsExplicitSelection &&
            !string.IsNullOrWhiteSpace(_selectedPanelKey) &&
            !string.Equals(_selectedPanelKey, "base", StringComparison.OrdinalIgnoreCase))
        {
            var selectedPanel = WhiteLabelContext.Panels.FirstOrDefault(panel =>
                string.Equals(panel.Key, _selectedPanelKey, StringComparison.OrdinalIgnoreCase));

            if (selectedPanel is not null && !string.IsNullOrWhiteSpace(selectedPanel.PathPrefix))
            {
                NavigationManager.NavigateTo($"/{selectedPanel.PathPrefix.Trim('/').ToLowerInvariant()}/", forceLoad: true);
                return;
            }
        }

        NavigationManager.NavigateTo(WhiteLabelContext.GetPath(), forceLoad: true);
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
}
