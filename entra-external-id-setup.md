# Entra External ID — Setup Guide & Disable Sign-Up Script

> **Environment:** Windows PC, PowerShell, existing Azure/Entra tenant  
> **Goal:** Configure Entra External ID (CIAM) on an existing tenant, create a test user, and disable self-service sign-up via the Graph API

---

## Prerequisites

- Access to [entra.microsoft.com](https://entra.microsoft.com) with an admin account on your existing tenant
- PowerShell 5.1 or later (built in to Windows — no install needed)
- The Microsoft Graph PowerShell module (installed in Part 5)

---

## Part 1 — Create the External Tenant

> Your existing tenant is a "workforce" tenant. Entra External ID requires a **separate** external tenant. You create it from within your existing tenant.

**1.** Go to [entra.microsoft.com](https://entra.microsoft.com) and sign in with your work admin account.

**2.** In the top-left, confirm you are in your **existing work tenant** (not a personal one).

**3.** Navigate to **Identity > Overview > Manage tenants**.

**4.** Click **+ Create**.

**5.** On the tenant type screen, select **External** and click **Continue**.

**6.** Fill in the Basics tab:
   - **Tenant Name** — e.g. `MyAppTest`
   - **Domain Name** — e.g. `myapptest` (becomes `myapptest.onmicrosoft.com`)
   - **Country/Region** — United Kingdom

**7.** Click **Review + Create**, review the details, then click **Create**.

**8.** Wait for provisioning to complete — watch the Notifications bell in the top bar. This can take up to 5 minutes.

---

## Part 2 — Switch Into the New External Tenant

**9.** Click the **Settings icon** (cog) in the top menu bar.

**10.** Under **Directories + subscriptions**, find `myapptest` in the list and click **Switch**.

**11.** The portal reloads. Confirm the tenant name in the top-left shows your new external tenant, badged as **External**.

> ⚠️ Every step from here until Part 6 must be done while switched into the **external tenant**, not your work tenant.

---

## Part 3 — Configure a User Flow

**12.** Navigate to **Identity > External Identities > User flows**.

**13.** Click **+ New user flow**.

**14.** On the Create screen:
   - **Name** — e.g. `SignUpSignIn`
   - Under **Identity providers**, tick **Email with password only**
   - Leave all phone/SMS options unticked

**15.** Under **User attributes**, select any attributes you want to collect at sign-up (at minimum: **Display Name**, **Email Address**).

**16.** Click **Create**.

**17.** Once created, click into the user flow and note its name — you will need it in Part 5.

---

## Part 4 — Register Your App

**18.** Navigate to **Identity > Applications > App registrations**.

**19.** Click **+ New registration**.

**20.** Fill in:
   - **Name** — e.g. `MyBlazorAppDev`
   - **Supported account types** — *Accounts in this organizational directory only*
   - **Redirect URI** — select **Web**, enter `https://localhost:5001/signin-oidc`

**21.** Click **Register**.

**22.** From the Overview page, copy and save these two values — you will need them in your `appsettings.json` and in the PowerShell script:
   - **Application (client) ID**
   - **Directory (tenant) ID**

**23.** Go to **Certificates & secrets** in the left menu.

**24.** Click **+ New client secret**.
   - Description: `dev`
   - Expiry: **24 months**

**25.** Click **Add**.

**26.** **Copy the secret Value immediately** — you cannot retrieve it again after navigating away.

**27.** Go to **API permissions** in the left menu.

**28.** Click **+ Add a permission > Microsoft Graph > Application permissions**.

**29.** Search for `User.ReadWrite.All` and tick it.

**30.** Click **Add permissions**.

**31.** Click **Grant admin consent for myapptest** and confirm. The permission should show a green tick.

**32.** Link the app to your user flow:
   - Navigate to **Identity > External Identities > User flows**
   - Select your user flow
   - Click **Applications** in the left menu
   - Click **+ Add application**
   - Select `MyBlazorAppDev` and confirm

---

## Part 5 — Create a Test User

> ⚠️ Use a different email address than your admin account. Using the same email creates a conflicting second account. A Gmail address or a `+test` variant (e.g. `you+test@gmail.com`) works fine.

**33.** Navigate to **Identity > Users**.

**34.** Click **+ New user > Create new external user**.

**35.** Fill in:
   - **Display name** — e.g. `Test User`
   - Under **Identities**, set **Sign-in type** to **Email** and enter the test email address
   - Set a **temporary password** — e.g. `Welcome123!`
   - Tick **Must change password at next sign-in**

**36.** Click **Create**.

**37.** Note the user's **Object ID** from the user overview page — store this in your database linked to your app's user record.

---

## Part 6 — Your Blazor appsettings.json

Replace the placeholder values with those you copied in steps 22 and 26:

```json
"AzureAd": {
  "Instance": "https://myapptest.ciamlogin.com/",
  "TenantId": "YOUR_TENANT_ID_GUID",
  "ClientId": "YOUR_CLIENT_ID_GUID",
  "ClientSecret": "YOUR_CLIENT_SECRET_VALUE",
  "CallbackPath": "/signin-oidc",
  "SignedOutCallbackPath": "/signout-callback-oidc"
}
```

> ⚠️ The `Instance` URL uses `ciamlogin.com`, **not** `login.microsoftonline.com`. Using the wrong instance is the most common cause of `unauthorized_client` errors.

---

## Part 7 — Disable Self-Service Sign-Up (PowerShell Script)

This script uses the Microsoft Graph beta API to set `isSignUpAllowed = false` on your user flow, preventing anyone from self-registering through the Entra login page.

### 7.1 — Install the Graph PowerShell module (one-time)

Open PowerShell **as Administrator** and run:

```powershell
Install-Module Microsoft.Graph -Scope CurrentUser -Force
```

This may take a few minutes.

### 7.2 — The script

Save this as `disable-entra-signup.ps1`. Fill in your values in the config block at the top.

```powershell
# ─────────────────────────────────────────────────────────
# disable-entra-signup.ps1
# Disables self-service sign-up on an Entra External ID
# user flow via the Microsoft Graph beta API
# ─────────────────────────────────────────────────────────

# ── Config — fill these in ────────────────────────────────
$TenantId  = "YOUR_TENANT_ID_GUID"
$ClientId  = "YOUR_APP_CLIENT_ID_GUID"
# ─────────────────────────────────────────────────────────

$ErrorActionPreference = "Stop"

function Write-Step  { param($msg) Write-Host "`n>> $msg" -ForegroundColor Cyan }
function Write-Ok    { param($msg) Write-Host "   OK  $msg" -ForegroundColor Green }
function Write-Fail  { param($msg) Write-Host "   FAIL  $msg" -ForegroundColor Red }
function Write-Warn  { param($msg) Write-Host "   WARN  $msg" -ForegroundColor Yellow }

Write-Host ""
Write-Host "════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Entra External ID — Disable Sign-Up" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════" -ForegroundColor Cyan

# ── Step 1: Check module ──────────────────────────────────
Write-Step "Step 1/4 — Checking Microsoft.Graph module..."
if (-not (Get-Module -ListAvailable -Name Microsoft.Graph)) {
    Write-Fail "Microsoft.Graph module not found."
    Write-Host "   Run this first: Install-Module Microsoft.Graph -Scope CurrentUser -Force" -ForegroundColor Yellow
    exit 1
}
Write-Ok "Microsoft.Graph module found"

# ── Step 2: Connect ───────────────────────────────────────
Write-Step "Step 2/4 — Connecting to Microsoft Graph..."
Write-Host "   A browser window will open — sign in with your external tenant admin account." -ForegroundColor Gray

Connect-MgGraph -TenantId $TenantId -Scopes "EventListener.ReadWrite.All" -NoWelcome

Write-Ok "Connected to tenant: $TenantId"

# ── Step 3: Find user flow ────────────────────────────────
Write-Step "Step 3/4 — Finding user flow for app client ID: $ClientId..."

$filter = "microsoft.graph.externalUsersSelfServiceSignUpEventsFlow/conditions/applications/includeApplications/any(appId:appId/appId eq '$ClientId')"
$encodedFilter = [Uri]::EscapeDataString($filter)
$uri = "https://graph.microsoft.com/beta/identity/authenticationEventsFlows?`$filter=$encodedFilter"

$response = Invoke-MgGraphRequest -Method GET -Uri $uri

if (-not $response.value -or $response.value.Count -eq 0) {
    Write-Fail "No user flow found for client ID: $ClientId"
    Write-Host ""
    Write-Host "   Possible causes:" -ForegroundColor Yellow
    Write-Host "   1. The CLIENT_ID does not match your app registration" -ForegroundColor Yellow
    Write-Host "   2. The app has not been linked to a user flow" -ForegroundColor Yellow
    Write-Host "      Fix: Identity > User flows > your flow > Applications > Add application" -ForegroundColor Yellow
    Write-Host "   3. You are connected to the wrong tenant" -ForegroundColor Yellow
    exit 1
}

$flow     = $response.value[0]
$flowId   = $flow.id
$flowName = $flow.displayName

Write-Ok "Found user flow: '$flowName'"
Write-Host "   Flow ID: $flowId" -ForegroundColor Gray

# ── Step 4: Disable sign-up ───────────────────────────────
Write-Step "Step 4/4 — Disabling self-service sign-up..."

$patchBody = @{
    "@odata.type" = "#microsoft.graph.externalUsersSelfServiceSignUpEventsFlow"
    "onInteractiveAuthFlowStart" = @{
        "@odata.type" = "#microsoft.graph.onInteractiveAuthFlowStartExternalUsersSelfServiceSignUp"
        "isSignUpAllowed" = $false
    }
} | ConvertTo-Json -Depth 5

Invoke-MgGraphRequest `
    -Method PATCH `
    -Uri "https://graph.microsoft.com/beta/identity/authenticationEventsFlows/$flowId" `
    -Body $patchBody `
    -ContentType "application/json"

Write-Ok "PATCH request succeeded"

# ── Verify ────────────────────────────────────────────────
Write-Host ""
Write-Host "   Verifying..." -ForegroundColor Gray

$verify = Invoke-MgGraphRequest `
    -Method GET `
    -Uri "https://graph.microsoft.com/beta/identity/authenticationEventsFlows/$flowId"

$isSignUpAllowed = $verify.onInteractiveAuthFlowStart.isSignUpAllowed

if ($isSignUpAllowed -eq $false) {
    Write-Ok "Verified: isSignUpAllowed = false"
} else {
    Write-Warn "Current value: isSignUpAllowed = $isSignUpAllowed"
    Write-Host "   The change may still be propagating. Check the portal in a few minutes." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Done!" -ForegroundColor Green
Write-Host "  Note: changes can take a few minutes to" -ForegroundColor Gray
Write-Host "  appear on the live login page." -ForegroundColor Gray
Write-Host "════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
```

### 7.3 — Run the script

Open PowerShell (does **not** need to be Administrator this time) and run:

```powershell
# If you get a script execution policy error, run this first (once):
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Then run the script:
.\disable-entra-signup.ps1
```

A browser window will open asking you to sign in — use your **external tenant** admin account (not your main work account).

---

## Verification

After running the script, confirm it worked:

1. Open your Entra login page in a **private/incognito browser window**
2. The sign-up link should no longer be visible
3. If it is still showing, wait 5–10 minutes and try again — propagation can be delayed

To re-enable sign-up at any time, change `"isSignUpAllowed" = $false` to `$true` in the script and run it again.

---

## Quick Reference — Values to Keep Safe

| Value | Where to find it |
|---|---|
| Tenant ID | App registration > Overview |
| Client ID | App registration > Overview |
| Client Secret | Saved at step 26 (cannot be retrieved again) |
| Instance URL | `https://myapptest.ciamlogin.com/` |
| Test user Object ID | Identity > Users > select user > Overview |

---

## Common Errors

| Error | Cause | Fix |
|---|---|---|
| `unauthorized_client` | App registered in wrong tenant | Re-register the app inside the external tenant |
| `SSL connection could not be established` | Wrong SMTP port/TLS settings | Use port 587 with `EnableSsl = true` |
| User flow not found in script | App not linked to user flow | Identity > User flows > Applications > Add application |
| Sign-up still showing after script | Propagation delay | Wait 5–10 minutes |
| `ForceChangePasswordNextSignIn` not prompting | Password policy not set | Ensure `PasswordPolicies = "DisablePasswordExpiration"` is set on user creation |
