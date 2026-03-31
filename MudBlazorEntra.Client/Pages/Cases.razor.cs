using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazorEntra.Client.Models.Cases;
using MudBlazorEntra.Client.Services;

namespace MudBlazorEntra.Client.Pages;

public partial class Cases
{
    [Parameter]
    public string? PanelPath { get; set; }

    private const string AbsoluteMode = "absolute";
    private const string RelativeMode = "relative";
    private const string LastUpdatedBasis = "updated";
    private const string CreatedBasis = "created";

    private IReadOnlyList<CaseListItem> _cases = CaseSampleDataService.Cases;
    private MudMenu? _dateFilterMenu;
    private OwnershipFilter _ownershipFilter = OwnershipFilter.MyCases;
    private string _searchTerm = string.Empty;
    private int _activeTabIndex;
    private string _dateMode = RelativeMode;
    private string _dateBasis = LastUpdatedBasis;
    private string _relativeRange = "3m";
    private DateTime? _absoluteFrom = DateTime.Today.AddMonths(-1);
    private DateTime? _absoluteTo = DateTime.Today;

    private IEnumerable<CaseListItem> FilteredCases => _cases
        .Where(MatchesOwnership)
        .Where(MatchesTab)
        .Where(MatchesSearch)
        .Where(MatchesDate)
        .OrderByDescending(GetBasisDate);

    private string DateFilterSummary =>
        _dateMode == AbsoluteMode
            ? $"Absolute: {FormatDate(_absoluteFrom)} to {FormatDate(_absoluteTo)} ({BasisLabel})"
            : $"Relative: {RelativeLabel} ({BasisLabel})";

    private string BasisLabel => _dateBasis == LastUpdatedBasis ? "Last updated" : "Created";

    private string RelativeLabel => _relativeRange switch
    {
        "3w" => "Last 3 weeks",
        "3m" => "Last 3 months",
        "6m" => "Last 6 months",
        "1y" => "Last 12 months",
        _ => "All time"
    };

    protected override async Task OnInitializedAsync()
    {
        _cases = await PortalDataService.GetPoliciesAsync();
        ApplyTabFromQuery();
    }

    protected override void OnParametersSet()
    {
        ApplyTabFromQuery();
    }

    private void SetOwnershipFilter(OwnershipFilter filter)
    {
        _ownershipFilter = filter;
    }

    private string GetOwnershipButtonClass(OwnershipFilter filter)
    {
        return filter == _ownershipFilter
            ? "ownership-option active"
            : "ownership-option";
    }

    private bool MatchesOwnership(CaseListItem item)
    {
        return _ownershipFilter == OwnershipFilter.CompanyCases || item.IsMine;
    }

    private bool MatchesTab(CaseListItem item)
    {
        return _activeTabIndex switch
        {
            1 => item.Status == "Draft",
            2 => item.Status == "Issued",
            3 => item.Status == "Cancelled",
            _ => true
        };
    }

    private bool MatchesSearch(CaseListItem item)
    {
        if (string.IsNullOrWhiteSpace(_searchTerm))
        {
            return true;
        }

        return item.YourReference.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
               item.CaseId.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
               item.Address.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase);
    }

    private bool MatchesDate(CaseListItem item)
    {
        var basisDate = GetBasisDate(item).Date;
        if (_dateMode == AbsoluteMode)
        {
            if (_absoluteFrom is not null && basisDate < _absoluteFrom.Value.Date)
            {
                return false;
            }

            if (_absoluteTo is not null && basisDate > _absoluteTo.Value.Date)
            {
                return false;
            }

            return true;
        }

        var fromDate = _relativeRange switch
        {
            "3w" => DateTime.Today.AddDays(-21),
            "3m" => DateTime.Today.AddMonths(-3),
            "6m" => DateTime.Today.AddMonths(-6),
            "1y" => DateTime.Today.AddYears(-1),
            _ => DateTime.MinValue
        };

        return basisDate >= fromDate.Date;
    }

    private DateTime GetBasisDate(CaseListItem item)
    {
        return _dateBasis == LastUpdatedBasis ? item.LastUpdatedDate : item.CreatedDate;
    }

    private async Task ApplyDateFilterAsync()
    {
        if (_dateFilterMenu is not null)
        {
            await _dateFilterMenu.CloseMenuAsync();
        }
    }

    private void ResetDateFilter()
    {
        _dateMode = RelativeMode;
        _dateBasis = LastUpdatedBasis;
        _relativeRange = "3m";
        _absoluteFrom = DateTime.Today.AddMonths(-1);
        _absoluteTo = DateTime.Today;
    }

    private void OnTabChanged(int index)
    {
        _activeTabIndex = index;
    }

    private void OnRowClick(TableRowClickEventArgs<CaseListItem> args)
    {
        if (args.Item is not null)
        {
            NavigationManager.NavigateTo(WhiteLabelContext.GetPath($"cases/{args.Item.CaseId}"));
        }
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

    private static string FormatDate(DateTime? value) => value?.ToString("dd MMM yyyy") ?? "Any";

    private void ApplyTabFromQuery()
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var tabValue = GetQueryParameter(uri.Query, "tab");
        var scopeValue = GetQueryParameter(uri.Query, "scope");

        _ownershipFilter = string.Equals(scopeValue, "company", StringComparison.OrdinalIgnoreCase)
            ? OwnershipFilter.CompanyCases
            : OwnershipFilter.MyCases;

        if (string.IsNullOrWhiteSpace(tabValue))
        {
            _activeTabIndex = 0;
            return;
        }

        _activeTabIndex = tabValue.ToLowerInvariant() switch
        {
            "draft" => 1,
            "issued" => 2,
            "cancelled" => 3,
            _ => 0
        };
    }

    private static string? GetQueryParameter(string queryString, string key)
    {
        if (string.IsNullOrWhiteSpace(queryString))
        {
            return null;
        }

        foreach (var segment in queryString.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = segment.Split('=', 2);
            if (parts.Length == 2 && string.Equals(parts[0], key, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return null;
    }

    private enum OwnershipFilter
    {
        MyCases,
        CompanyCases
    }
}
