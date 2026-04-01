using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazorEntra.Client.Models.NewEnquiry;
using MudBlazorEntra.Client.Services;

namespace MudBlazorEntra.Client.Pages;

public partial class NewEnquiry
{
    private const int LastStepIndex = 2;

    [Parameter]
    public string? PanelPath { get; set; }

    [SupplyParameterFromQuery(Name = "step")]
    public string? Step { get; set; }

    private IReadOnlyList<BreadcrumbItem> _breadcrumbs =>
    [
        new("Home", href: WhiteLabelContext.GetPath(), disabled: false),
        new("New enquiry", href: null, disabled: true)
    ];

    private int _activeStep;
    private string _riskSearchText = string.Empty;
    private readonly List<RiskOption> _riskOptions =
    [
        new() { Id = "continued-use-1", Name = "Risk Name", Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.", IsSelected = false },
        new() { Id = "continued-use-2", Name = "Risk Name", Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.", IsSelected = false },
        new() { Id = "continued-use-3", Name = "Risk Name", Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.", IsSelected = false, IsDisabled = true },
        new() { Id = "continued-use-4", Name = "Risk Name", Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.", IsSelected = false },
        new() { Id = "continued-use-5", Name = "Chancel Repair Risk Name", Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.", Tag = "Exclusive Risk", IsSelected = true },
        new() { Id = "continued-use-6", Name = "Risk Name", Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.", IsSelected = false },
        new() { Id = "continued-use-7", Name = "Risk Name", Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.", IsSelected = false, IsDisabled = true },
        new() { Id = "continued-use-8", Name = "Risk Name", Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.", IsSelected = false },
        new() { Id = "continued-use-9", Name = "Risk Name", Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.", IsSelected = false },
        new() { Id = "continued-use-10", Name = "Risk Name", Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.", IsSelected = false, IsDisabled = true },
        new() { Id = "continued-use-11", Name = "Risk Name", Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.", IsSelected = false },
        new() { Id = "continued-use-12", Name = "Risk Name", Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.", IsSelected = false }
    ];

    [Inject]
    private WhiteLabelContext WhiteLabelContext { get; set; } = default!;

    private IReadOnlyList<RiskOption> FilteredRisks => _riskOptions
        .Where(x => string.IsNullOrWhiteSpace(_riskSearchText) ||
                    x.Name.Contains(_riskSearchText, StringComparison.OrdinalIgnoreCase))
        .ToList();

    private int SelectedRiskCount => _riskOptions.Count(x => x.IsSelected);

    protected override void OnParametersSet()
    {
        _activeStep = ParseStep(Step);
    }

    private string GetStepUrl(int stepIndex)
    {
        var boundedStepIndex = Math.Clamp(stepIndex, 0, LastStepIndex);
        return WhiteLabelContext.GetPath($"new-enquiry?step={GetStepQueryValue(boundedStepIndex)}");
    }

    private static string GetStepLabel(int stepIndex)
    {
        return stepIndex switch
        {
            1 => "Risk Selection",
            2 => "Policy Details",
            _ => "Product Selection"
        };
    }

    private Task OnRiskSearchTextChanged(string value)
    {
        _riskSearchText = value;
        return Task.CompletedTask;
    }

    private Task OnRiskSelectedChanged((string RiskId, bool IsSelected) update)
    {
        var risk = _riskOptions.FirstOrDefault(x => x.Id == update.RiskId);
        if (risk is not null)
        {
            risk.IsSelected = update.IsSelected;
        }

        return Task.CompletedTask;
    }

    private static int ParseStep(string? stepValue)
    {
        return stepValue?.ToLowerInvariant() switch
        {
            "risk" => 1,
            "details" => 2,
            _ => 0
        };
    }

    private static string GetStepQueryValue(int stepIndex)
    {
        return stepIndex switch
        {
            1 => "risk",
            2 => "details",
            _ => "product"
        };
    }
}
