using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazorEntra.Client.Models.Policies;
using MudBlazorEntra.Client.Services;

namespace MudBlazorEntra.Client.Pages;

public partial class CaseDetails
{
    [Parameter]
    public string? PanelPath { get; set; }

    [Parameter]
    public string CaseId { get; set; } = string.Empty;

    private PolicyDetailsResponse? _case;
    private bool _isLoading = true;
    private IReadOnlyList<BreadcrumbItem> Breadcrumbs =>
    [
        new("Cases", href: WhiteLabelContext.GetPath("cases")),
        new("Case details", href: null, disabled: true)
    ];

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        _case = await PortalDataService.GetPolicyByIdAsync(CaseId);
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
}
