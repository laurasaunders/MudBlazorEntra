using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using MudBlazorEntra.Client;
using MudBlazorEntra.Client.Models.Authentication;
using MudBlazorEntra.Client.Models.WhiteLabel;
using MudBlazorEntra.Client.Services;
using MudBlazorEntra.Components;
using MudBlazorEntra.Options;
using MudBlazorEntra.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp =>
{
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var baseUri = httpContext is null
        ? "https://localhost/"
        : $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/";

    return new HttpClient
    {
        BaseAddress = new Uri(baseUri)
    };
});

builder.Services.Configure<EntraAuthenticationOptions>(builder.Configuration.GetSection(EntraAuthenticationOptions.SectionName));
builder.Services.Configure<GraphRegistrationOptions>(builder.Configuration.GetSection(GraphRegistrationOptions.SectionName));
builder.Services.Configure<GraphApiOptions>(builder.Configuration.GetSection(GraphApiOptions.SectionName));
builder.Services.Configure<FirstTitleOnlineApiOptions>(builder.Configuration.GetSection(FirstTitleOnlineApiOptions.SectionName));
builder.Services.Configure<EmailDeliveryOptions>(builder.Configuration.GetSection(EmailDeliveryOptions.SectionName));
builder.Services.Configure<WhiteLabelOptions>(builder.Configuration.GetSection(WhiteLabelOptions.SectionName));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "__Host-MudBlazorEntra.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddOpenIdConnect(options =>
    {
        var entraOptions = builder.Configuration
            .GetSection(EntraAuthenticationOptions.SectionName)
            .Get<EntraAuthenticationOptions>() ?? new EntraAuthenticationOptions();

        options.Authority = $"{entraOptions.Instance.TrimEnd('/')}/{entraOptions.TenantId}/v2.0";
        options.ClientId = entraOptions.ClientId;
        options.ClientSecret = entraOptions.ClientSecret;
        options.CallbackPath = entraOptions.CallbackPath;
        options.SignedOutCallbackPath = entraOptions.SignedOutCallbackPath;
        options.ResponseType = "code";
        options.SaveTokens = false;
        options.MapInboundClaims = false;
        options.GetClaimsFromUserInfoEndpoint = false;
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.TokenValidationParameters.NameClaimType = "name";
    });

builder.Services.AddSingleton<EntraAccessTokenProvider>();
builder.Services.AddScoped<CurrentUserContextAccessor>();
builder.Services.AddScoped<IRegistrationEligibilityService, AllowAllRegistrationEligibilityService>();
builder.Services.AddScoped<IRegistrationEmailSender, SmtpRegistrationEmailSender>();
builder.Services.AddScoped<PortalDataService>();
builder.Services.AddScoped<FirstTitleOnlineApi>();
builder.Services.AddScoped<CachedUserDetailsService>();
builder.Services.AddScoped<WhiteLabelContext>();
builder.Services.AddHttpClient<GraphRegistrationService>();
builder.Services.AddHttpClient<ProtectedApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.Use(async (httpContext, next) =>
{
    var whiteLabelOptions = httpContext.RequestServices
        .GetRequiredService<IOptions<WhiteLabelOptions>>()
        .Value;

    var requestPath = httpContext.Request.Path;
    var selectedPanelKey = httpContext.Request.Query["panel"].ToString();
    if (string.IsNullOrWhiteSpace(selectedPanelKey))
    {
        selectedPanelKey = httpContext.Request.Cookies[WhiteLabelContext.PanelCookieName];
    }

    var originalPath = requestPath.Value?.Trim('/') ?? string.Empty;
    var currentPanel = !string.IsNullOrWhiteSpace(selectedPanelKey)
        ? whiteLabelOptions.Panels.FirstOrDefault(panel =>
            string.Equals(panel.Key, selectedPanelKey, StringComparison.OrdinalIgnoreCase))
        : null;

    currentPanel ??= whiteLabelOptions.Panels.FirstOrDefault(panel =>
                        !string.IsNullOrWhiteSpace(panel.PathPrefix) &&
                        (string.Equals(panel.PathPrefix.Trim('/'), originalPath, StringComparison.OrdinalIgnoreCase) ||
                         originalPath.StartsWith($"{panel.PathPrefix.Trim('/')}/", StringComparison.OrdinalIgnoreCase)))
                    ?? whiteLabelOptions.Panels.FirstOrDefault(panel => string.IsNullOrWhiteSpace(panel.PathPrefix));

    httpContext.Response.Cookies.Append(
        WhiteLabelContext.PanelCookieName,
        currentPanel?.Key ?? "base",
        new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Path = "/"
        });

    await next();
});

app.UseAuthentication();

app.Use(async (httpContext, next) =>
{
    if (httpContext.User.Identity?.IsAuthenticated == true)
    {
        await next();
        return;
    }

    if (!HttpMethods.IsGet(httpContext.Request.Method))
    {
        await next();
        return;
    }

    var path = httpContext.Request.Path.Value ?? "/";
    if (path.StartsWith("/signin", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/authentication/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/_content", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/images", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase))
    {
        await next();
        return;
    }

    var whiteLabelOptions = httpContext.RequestServices
        .GetRequiredService<IOptions<WhiteLabelOptions>>()
        .Value;

    if (string.Equals(path, "/", StringComparison.OrdinalIgnoreCase))
    {
        httpContext.Response.Redirect("/signin");
        return;
    }

    var trimmedPath = path.Trim('/');
    var matchedPanel = whiteLabelOptions.Panels.FirstOrDefault(panel =>
        !string.IsNullOrWhiteSpace(panel.PathPrefix) &&
        string.Equals(panel.PathPrefix.Trim('/'), trimmedPath, StringComparison.OrdinalIgnoreCase));

    if (matchedPanel is not null)
    {
        httpContext.Response.Redirect($"/{matchedPanel.PathPrefix.Trim('/').ToLowerInvariant()}/signin");
        return;
    }

    await next();
});

app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(MudBlazorEntra.Client._Imports).Assembly);

app.MapGet("/authentication/login", async (HttpContext httpContext, string? returnUrl) =>
{
    var redirectUri = string.IsNullOrWhiteSpace(returnUrl)
        ? "/"
        : returnUrl;
    await httpContext.ChallengeAsync(
        OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = redirectUri });
}).AllowAnonymous();

app.MapGet("/authentication/logout", async (HttpContext httpContext, string? returnUrl) =>
{
    var redirectUri = string.IsNullOrWhiteSpace(returnUrl)
        ? "/"
        : returnUrl;
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await httpContext.SignOutAsync(
        OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = redirectUri });
});

app.MapGet("/site-version/select/{panelKey}", (HttpContext httpContext, string panelKey, IOptions<WhiteLabelOptions> whiteLabelOptionsAccessor) =>
{
    var panel = whiteLabelOptionsAccessor.Value.Panels.FirstOrDefault(candidate =>
        string.Equals(candidate.Key, panelKey, StringComparison.OrdinalIgnoreCase));

    if (panel is null)
    {
        return Results.Redirect("/site-version");
    }

    httpContext.Response.Cookies.Append(
        WhiteLabelContext.PanelCookieName,
        panel.Key,
        new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Path = "/"
        });

    var redirectPath = string.IsNullOrWhiteSpace(panel.PathPrefix)
        ? "/"
        : $"/{panel.PathPrefix.Trim('/').ToLowerInvariant()}/";

    return Results.Redirect(redirectPath);
}).RequireAuthorization();

app.MapGet("/api/site-version/current", (HttpContext httpContext) =>
{
    return Results.Ok(new CurrentSiteVersionResponse
    {
        PanelKey = httpContext.Request.Cookies[WhiteLabelContext.PanelCookieName] ?? string.Empty
    });
}).RequireAuthorization();

app.MapPost("/api/account/register", async (RegisterUserRequest request, GraphRegistrationService graphRegistrationService, CancellationToken cancellationToken) =>
{
    try
    {
        var result = await graphRegistrationService.RegisterAsync(request, cancellationToken);
        return Results.Ok(result);
    }
    catch (RegistrationConflictException ex)
    {
        return Results.Conflict(new RegisterUserResponse
        {
            Email = request.Email,
            Message = ex.Message
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new RegisterUserResponse
        {
            Email = request.Email,
            Message = ex.Message
        });
    }
})
.AllowAnonymous()
.DisableAntiforgery();

app.MapGet("/api/account/me", (HttpContext httpContext, CurrentUserContextAccessor currentUserContextAccessor) =>
{
    var user = httpContext.User;
    return Results.Ok(new CurrentUserInfoResponse
    {
        UserId = currentUserContextAccessor.GetUserId(user),
        DisplayName = user.Identity?.Name ?? string.Empty,
        Email = user.FindFirst("preferred_username")?.Value
            ?? user.FindFirst("email")?.Value
            ?? string.Empty
    });
}).RequireAuthorization();

app.MapGet("/api/users/{userId}/details", async (string userId, CachedUserDetailsService cachedUserDetailsService) =>
{
    var result = await cachedUserDetailsService.GetUserDetailsAsync(userId);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapGet("/api/policies", async (FirstTitleOnlineApi firstTitleOnlineApi) =>
{
    var result = await firstTitleOnlineApi.GetAllPoliciesAsync();
    return Results.Ok(result);
}).RequireAuthorization();

app.MapGet("/api/policies/{policyId}", async (string policyId, FirstTitleOnlineApi firstTitleOnlineApi) =>
{
    var result = await firstTitleOnlineApi.GetPolicyByIdAsync(policyId);
    return Results.Ok(result);
}).RequireAuthorization();

app.Run();
