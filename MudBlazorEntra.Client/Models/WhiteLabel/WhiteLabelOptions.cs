namespace MudBlazorEntra.Client.Models.WhiteLabel;

public class WhiteLabelOptions
{
    public const string SectionName = "WhiteLabel";

    public string DefaultPanelKey { get; set; } = "base";
    public List<WhiteLabelPanelOptions> Panels { get; set; } = [];
}

public class WhiteLabelPanelOptions
{
    public string Key { get; set; } = string.Empty;
    public string PathPrefix { get; set; } = string.Empty;
    public string DisplayName { get; set; } = "CaseFlow";
    public string LogoPath { get; set; } = "/images/logo-placeholder.svg";
    public string PrimaryColor { get; set; } = "#0075C3";
    public string SecondaryColor { get; set; } = "#00ADEF";
    public string TertiaryColor { get; set; } = "#EBBD5F";
    public string BackgroundColor { get; set; } = "#F4F4F4";
    public string SurfaceColor { get; set; } = "#ffffff";
    public string SurfaceAccentColor { get; set; } = "#E9EEF2";
    public string BorderColor { get; set; } = "#DCEFF9";
    public string TextPrimaryColor { get; set; } = "#141D3A";
    public string TextSecondaryColor { get; set; } = "#2F2F2F";
    public string AppBarColor { get; set; } = "#003763";
    public string FooterColor { get; set; } = "#003763";
    public string NavTextColor { get; set; } = "#F9FAFB";
    public string PrimaryButtonColor { get; set; } = "#0075C3";
    public string PrimaryButtonTextColor { get; set; } = "#ffffff";
    public string SecondaryButtonColor { get; set; } = "#141D3A";
    public string SecondaryButtonTextColor { get; set; } = "#ffffff";
    public string TertiaryButtonColor { get; set; } = "#141D3A";
    public string TertiaryButtonTextColor { get; set; } = "#141D3A";
    public string AccentTextColor { get; set; } = "#0075C3";
    public string MetricCardPrimaryStart { get; set; } = "#003763";
    public string MetricCardPrimaryEnd { get; set; } = "#0075C3";
    public string MetricCardSecondaryStart { get; set; } = "#013A6F";
    public string MetricCardSecondaryEnd { get; set; } = "#00ADEF";
    public string MetricCardTertiaryStart { get; set; } = "#0075C3";
    public string MetricCardTertiaryEnd { get; set; } = "#4EADE1";
    public string MetricCardQuaternaryStart { get; set; } = "#141D3A";
    public string MetricCardQuaternaryEnd { get; set; } = "#013A6F";
}
