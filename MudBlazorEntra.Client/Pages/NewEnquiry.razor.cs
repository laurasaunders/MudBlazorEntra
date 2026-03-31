using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazorEntra.Client.Services;

namespace MudBlazorEntra.Client.Pages;

public partial class NewEnquiry
{
    private const int LastStepIndex = 2;

    [Parameter]
    public string? PanelPath { get; set; }

    [SupplyParameterFromQuery(Name = "step")]
    public string? Step { get; set; }

    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("New enquiry", href: null, disabled: true)
    ];

    private int _activeStep;

    [Inject]
    private WhiteLabelContext WhiteLabelContext { get; set; } = default!;

    protected override void OnParametersSet()
    {
        _activeStep = ParseStep(Step);
    }

    private string GetStepUrl(int stepIndex)
    {
        var boundedStepIndex = Math.Clamp(stepIndex, 0, LastStepIndex);
        return WhiteLabelContext.GetPath($"new-enquiry?step={GetStepQueryValue(boundedStepIndex)}");
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
