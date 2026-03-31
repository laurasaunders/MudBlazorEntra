using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazorEntra.Client.Models.Cases;
using MudBlazorEntra.Client.Models.Users;
using MudBlazorEntra.Client.Services;

namespace MudBlazorEntra.Client.Pages;

public partial class Dashboard
{
    [Parameter]
    public string? PanelPath { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("Dashboard", href: null, disabled: true)
    ];

    private IReadOnlyList<CaseListItem> _policies = CaseSampleDataService.Cases;
    private UserDetailsResponse? _userDetails;
    private bool _isLoading = true;

    private IReadOnlyList<CaseListItem> MyRecentCases => _policies.Where(x => x.IsMine).OrderByDescending(x => x.LastUpdatedDate).Take(5).ToList();
    private IReadOnlyList<CaseListItem> CompanyRecentCases => _policies.OrderByDescending(x => x.LastUpdatedDate).Take(5).ToList();
    private int AllPoliciesCount => _policies.Count;
    private int DraftPoliciesCount => _policies.Count(x => x.Status == "Draft");
    private int IssuedPoliciesCount => _policies.Count(x => x.Status == "Issued");
    private string PanelsSummary => _userDetails?.Panels is { Count: > 0 } panels ? string.Join(", ", panels) : "Not available";

    protected override async Task OnInitializedAsync()
    {
        _userDetails = await PortalDataService.GetCurrentUserDetailsAsync();
        _policies = await PortalDataService.GetPoliciesAsync();
        _isLoading = false;
    }

    private static string GetStatusChipClass(string status)
    {
        return status switch
        {
            "Draft" => "status-chip status-chip-draft",
            "Issued" => "status-chip status-chip-issued",
            "Cancelled" => "status-chip status-chip-cancelled",
            _ => "status-chip"
        };
    }

    private void OnCaseRowClick(TableRowClickEventArgs<CaseListItem> args)
    {
        if (args.Item is not null)
        {
            NavigationManager.NavigateTo(WhiteLabelContext.GetPath($"cases/{args.Item.CaseId}"));
        }
    }
}
