using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using MudBlazorEntra.Client.Models.WhiteLabel;

namespace MudBlazorEntra.Client.Services;

public class WhiteLabelContext(NavigationManager navigationManager, IOptions<WhiteLabelOptions> options)
{
    public const string PanelCookieName = "__Host-MudBlazorEntra.Panel";

    private readonly NavigationManager _navigationManager = navigationManager;
    private readonly WhiteLabelOptions _options = options.Value;

    public IReadOnlyList<WhiteLabelPanelOptions> Panels => _options.Panels;
    public WhiteLabelPanelOptions CurrentPanel => ResolveCurrentPanel();
    public bool IsExplicitSelection => ResolveIsExplicitSelection();

    public string RootPath(string relativePath = "")
    {
        var trimmedPath = relativePath.Trim().TrimStart('/');
        return string.IsNullOrEmpty(trimmedPath) ? "/" : $"/{trimmedPath}";
    }

    public string BaseHref
    {
        get
        {
            var prefix = NormalizePrefix(CurrentPanel.PathPrefix);
            return string.IsNullOrEmpty(prefix) ? "/" : $"{prefix}/";
        }
    }

    public string GetPath(string relativePath = "")
    {
        var trimmedPath = relativePath.Trim();
        if (string.IsNullOrEmpty(trimmedPath))
        {
            return BaseHref;
        }

        var prefix = NormalizePrefix(CurrentPanel.PathPrefix);
        var localPath = trimmedPath.TrimStart('/');
        return string.IsNullOrEmpty(prefix) ? $"/{localPath}" : $"{prefix}/{localPath}";
    }

    private WhiteLabelPanelOptions ResolveCurrentPanel()
    {
        var absoluteUri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);
        var path = absoluteUri.AbsolutePath.Trim('/');
        if (!string.IsNullOrWhiteSpace(path))
        {
            var matchedPanel = _options.Panels.FirstOrDefault(panel =>
                !string.IsNullOrWhiteSpace(panel.PathPrefix) &&
                (string.Equals(panel.PathPrefix.Trim('/'), path, StringComparison.OrdinalIgnoreCase) ||
                 path.StartsWith($"{panel.PathPrefix.Trim('/')}/", StringComparison.OrdinalIgnoreCase)));
            if (matchedPanel is not null)
            {
                return matchedPanel;
            }
        }

        return _options.Panels.FirstOrDefault(panel =>
                   string.Equals(panel.Key, _options.DefaultPanelKey, StringComparison.OrdinalIgnoreCase))
               ?? _options.Panels.FirstOrDefault(panel => string.IsNullOrWhiteSpace(panel.PathPrefix))
               ?? new WhiteLabelPanelOptions();
    }

    private bool ResolveIsExplicitSelection()
    {
        var absoluteUri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);
        var path = absoluteUri.AbsolutePath.Trim('/');
        return _options.Panels.Any(panel =>
            !string.IsNullOrWhiteSpace(panel.PathPrefix) &&
            (string.Equals(panel.PathPrefix.Trim('/'), path, StringComparison.OrdinalIgnoreCase) ||
             path.StartsWith($"{panel.PathPrefix.Trim('/')}/", StringComparison.OrdinalIgnoreCase)));
    }

    private static string NormalizePrefix(string prefix)
    {
        var trimmed = prefix.Trim().Trim('/');
        return string.IsNullOrEmpty(trimmed) ? string.Empty : $"/{trimmed.ToLowerInvariant()}";
    }
}
