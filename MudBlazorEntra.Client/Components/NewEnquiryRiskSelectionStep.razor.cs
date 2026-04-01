using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazorEntra.Client.Models.NewEnquiry;

namespace MudBlazorEntra.Client.Components;

public partial class NewEnquiryRiskSelectionStep
{
    private string LevelOfIndemnity { get; set; } = "100000";

    private string PolicyInFavourOf { get; set; } = "Lender";

    private bool _premiumCalculated;
    public CultureInfo _en = CultureInfo.GetCultureInfo("en-GB");

    [Parameter]
    public string SearchText { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> SearchTextChanged { get; set; }

    [Parameter]
    public IReadOnlyList<RiskOption> Risks { get; set; } = [];

    [Parameter]
    public EventCallback<(string RiskId, bool IsSelected)> OnRiskSelectedChanged { get; set; }

    [Parameter]
    public int SelectedRiskCount { get; set; }

    [Parameter]
    public string ContinueUrl { get; set; } = string.Empty;

    private void CalculatePremium()
    {
        _premiumCalculated = true;
    }

    private async Task OnRiskSelectionChangedAsync(string riskId, bool isSelected)
    {
        ResetPremium();
        await OnRiskSelectedChanged.InvokeAsync((riskId, isSelected));
    }

    private Task OnLevelOfIndemnityChanged(string value)
    {
        LevelOfIndemnity = value;
        ResetPremium();
        return Task.CompletedTask;
    }

    private Task OnPolicyInFavourOfChanged(string value)
    {
        PolicyInFavourOf = value;
        ResetPremium();
        return Task.CompletedTask;
    }

    private void ResetPremium()
    {
        _premiumCalculated = false;
    }
}
