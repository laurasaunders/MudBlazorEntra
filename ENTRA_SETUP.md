# Entra setup

## What this app now expects

- A confidential client app registration for the web app sign-in.
- Microsoft Graph application permission to send Entra invitations.
- A downstream API app registration or exposed scope for the server-to-server API call path.

The app uses:

- OpenID Connect sign-in for users.
- An encrypted ASP.NET auth cookie in the browser.
- Client credentials on the server for Microsoft Graph and your downstream API.

## Local redirect URIs

Add these redirect URIs to the Entra app registration:

- `https://localhost:7067/signin-oidc`
- `https://localhost:7067/signout-callback-oidc`

## Required configuration

Set these values with user secrets or environment variables instead of committing secrets to `appsettings.json`.

```bash
dotnet user-secrets set "Entra:TenantId" "<tenant-id>" --project MudBlazorEntra/MudBlazorEntra.csproj
dotnet user-secrets set "Entra:ClientId" "<client-id>" --project MudBlazorEntra/MudBlazorEntra.csproj
dotnet user-secrets set "Entra:ClientSecret" "<client-secret>" --project MudBlazorEntra/MudBlazorEntra.csproj
dotnet user-secrets set "GraphRegistration:InviteRedirectUrl" "https://localhost:7067/" --project MudBlazorEntra/MudBlazorEntra.csproj
dotnet user-secrets set "DownstreamApi:BaseUrl" "https://your-api.example.com/" --project MudBlazorEntra/MudBlazorEntra.csproj
dotnet user-secrets set "DownstreamApi:Scope" "api://<your-api-app-id>/.default" --project MudBlazorEntra/MudBlazorEntra.csproj
```

## Microsoft Graph permission

Grant the confidential client app an application permission that allows invitations. In most tenants this is:

- `User.Invite.All`

After adding it, grant admin consent.

If your tenant policy requires broader directory write permissions for your chosen registration model, adjust the Graph permission set accordingly.

## Downstream API permission

Expose an application scope on the target API and grant the confidential client app access to it. The config should then use:

- `api://<api-app-id>/.default`

## Registration flow implemented here

The register form does not create a local password account. It creates a Microsoft Entra invitation through Microsoft Graph and sends the Entra email to the user.

That is the usual fit for external user onboarding. If you need true member-account creation inside the tenant instead, the registration service must be changed to a different Graph flow.

## Useful endpoints

- `GET /authentication/login`
- `GET /authentication/logout`
- `POST /api/account/register`
- `GET /api/account/me`
