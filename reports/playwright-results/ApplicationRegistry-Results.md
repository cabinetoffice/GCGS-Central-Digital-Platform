# Playwright Test Results — Application Registry

| | |
|---|---|
| **Suite** | `Category=ApplicationRegistry` |
| **Run date** | 3 June 2026  00:57:01 UTC |
| **Duration** | 0.83 seconds |
| **Environment** | Docker · `cdp-sirsi` network |
| **Target** | `http://organisation-app:8080` |
| **Result file** | `reports/playwright-results/ApplicationRegistry.trx` |

---

## Summary

```
┌─────────┬────────┬────────┬─────────┬──────────┐
│  Total  │ Passed │ Failed │ Skipped │ Duration │
├─────────┼────────┼────────┼─────────┼──────────┤
│   10    │   0    │  10    │    0    │  0.83s   │
└─────────┴────────┴────────┴─────────┴──────────┘
```

> **Status: ❌ BLOCKED — all tests failed in `[OneTimeSetUp]` before any test body executed**
>
> Root cause: GOV.UK One Login credentials are not configured. The login flow enters a redirect loop (`ERR_TOO_MANY_REDIRECTS`) and crashes both test fixtures before a single assertion runs. No screenshots or Playwright traces were captured (these only fire from `[TearDown]`, which never ran).

---

## Fixtures

### 1 · `ApplicationRegistryNavigationTests` ❌ 3/3 failed

Tests that verify page routing, headings, and navigation links between Application Registry screens.

| # | Test | Duration | Outcome | Failure stage |
|---|---|---|---|---|
| 1 | `ApplicationListPage_LoadsCorrectly_ForBuyerOrg` | 81.7 ms | ❌ Failed | `[OneTimeSetUp]` |
| 2 | `ApplicationListPage_ShowsManageApplicationAccessLink_InDashboard` | 81.7 ms | ❌ Failed | `[OneTimeSetUp]` |
| 3 | `ApplicationListPage_ShowsTable_WhenAppsAvailable` | 81.7 ms | ❌ Failed | `[OneTimeSetUp]` |

---

### 2 · `ApplicationRegistryFunctionalTests` ❌ 7/7 failed

End-to-end journeys: application detail tabs, assign-user flow, edit roles, revoke access, validation.

| # | Test | Duration | Outcome | Failure stage |
|---|---|---|---|---|
| 4 | `ApplicationDetailPage_ShowsRolesAndPermissionsTabs` | 371.7 ms | ❌ Failed | `[OneTimeSetUp]` |
| 5 | `ApplicationDetailPage_PermissionsTab_LoadsCorrectly` | 371.4 ms | ❌ Failed | `[OneTimeSetUp]` |
| 6 | `UserAssignmentsPage_LoadsAndShowsAssignUserButton` | 371.7 ms | ❌ Failed | `[OneTimeSetUp]` |
| 7 | `AssignUserPage_LoadsWithUserPickerAndRoleCheckboxes` | 371.7 ms | ❌ Failed | `[OneTimeSetUp]` |
| 8 | `AssignUserJourney_HappyPath_AssignsThenRevokes` | 371.7 ms | ❌ Failed | `[OneTimeSetUp]` |
| 9 | `AssignUserPage_ValidationError_WhenNoSelectionMade` | 371.7 ms | ❌ Failed | `[OneTimeSetUp]` |
| 10 | `RevokeConfirmationPage_SelectNo_CancelsRevoke` | 371.7 ms | ❌ Failed | `[OneTimeSetUp]` |

> **Note on timing:** `ApplicationRegistryFunctionalTests` tests (~372 ms each) took longer than navigation tests (~82 ms) because the functional fixture attempted a real Playwright browser launch and navigation before the redirect loop was detected. Navigation tests were queued after and failed faster as the error was already cached.

---

## Root Cause

All 10 failures share a single root cause — **missing One Login credentials**.

```
Microsoft.Playwright.PlaywrightException
  net::ERR_TOO_MANY_REDIRECTS at http://organisation-app:8080/
```

**Failure chain:**

```
BaseTest.[OneTimeSetUp]
  └─ OneTimeLoginPerFixture()              ← fires once per test fixture
       └─ LoginPage.Login(url, email, password, secretKey)
            └─ page.GotoAsync("http://organisation-app:8080/")
                 └─ ERR_TOO_MANY_REDIRECTS
                      ← app redirects to One Login
                      ← One Login redirects back (no client ID configured)
                      ← infinite loop → browser gives up
```

**Why all 10 fail, not just 1:**  
`[OneTimeSetUp]` runs once per *fixture class*. There are 2 fixture classes (navigation + functional). Each failed at setup, marking all tests in that fixture as failed before any test body ran.

**Condensed stack trace:**

```
at LoginPage.Login(loginUrl, email, password, secretKey)
   Pages/LoginPage.cs:28

at BaseTest.OneTimeLoginPerFixture()
   Tests/BaseTest.cs:55

at NUnit OneTimeSetUpCommand...
```

---

## What is and isn't confirmed

### ✅ Confirmed (from parallel API + route checks during this run)

| Check | Result |
|---|---|
| All 6 new OrganisationApp routes registered | ✅ All return `302` (not `404`) |
| All AppRegistry API routes registered | ✅ All return `401` (not `404`) |
| `GET /api/organisations/{id}/applications` in OpenAPI spec | ✅ Confirmed present |
| MongoDB container healthy | ✅ `ping.ok = 1` |
| Docker stack (20 containers) healthy | ✅ All healthy |
| E2E test suite compiles and is discovered | ✅ 10 tests found |

### ❌ Not yet confirmed (requires credentials)

| Check | Blocked by |
|---|---|
| "Manage application access" link visible for Buyer org | One Login credentials |
| ApplicationList page renders with GDS table | One Login credentials |
| ApplicationDetail Roles/Permissions tabs work | One Login credentials |
| Assign user → check answers → confirm journey | One Login credentials + seeded AppRegistry data |
| Edit roles journey | One Login credentials + existing assignment |
| Revoke → confirm journey | One Login credentials + existing assignment |
| Validation error on blank form submit | One Login credentials |

---

## How to unblock

### Step 1 — Create `E2ETests/appsettings.Development.json`

```json
{
  "TestSettings": {
    "BaseUrl":   "http://localhost:8090",
    "ApiUrl":    "http://localhost:8082",
    "Email":     "your-onelogin-test@email.com",
    "Password":  "your-password",
    "SecretKey": "your-totp-base32-secret",
    "Headless":  false,
    "DatabaseConnectionString": "Server=localhost;Database=cdp;User Id=cdp_user;Password=cdp123;",
    "TestSupportAdminEmail":    "admin@email.com",
    "TestSupportAdminPassword": "admin-password",
    "TestSupportAdminSecretKey":"admin-totp-secret"
  }
}
```

### Step 2 — Run locally (headed, so you can watch)

```bash
cd E2ETests
dotnet test --filter "Category=ApplicationRegistry" \
  --results-directory ./TestResults \
  --logger "trx;LogFileName=ApplicationRegistry.trx"
```

### Step 3 — Or run in Docker with credentials + mounted output

```bash
docker run --rm \
  --network cdp-sirsi \
  --shm-size=1gb \
  -v "$(pwd)/reports/playwright-results:/app/TestResults" \
  -e TestSettings__BaseUrl=http://organisation-app:8080 \
  -e TestSettings__ApiUrl=http://organisation:8080 \
  -e TestSettings__Headless=true \
  -e TestSettings__Email="$E2E_EMAIL" \
  -e TestSettings__Password="$E2E_PASSWORD" \
  -e TestSettings__SecretKey="$E2E_SECRET" \
  -e TestSettings__DatabaseConnectionString="Server=db;Database=cdp;User Id=cdp_user;Password=cdp123;" \
  e2e-tests \
  dotnet test E2ETests.csproj \
    --filter "Category=ApplicationRegistry" \
    --results-directory /app/TestResults \
    --logger "trx;LogFileName=ApplicationRegistry.trx"
```

---

## Artefact locations

| Artefact | Path | When created |
|---|---|---|
| TRX result file | `reports/playwright-results/ApplicationRegistry.trx` | Every run |
| Screenshots on failure | `TestResults/Screenshots/{TestName}.png` | Only if test body reaches `[TearDown]` |
| Playwright traces on failure | `TestResults/Traces/{TestName}.zip` | Only if test body reaches `[TearDown]` |

**Viewing a Playwright trace:**

```bash
npx playwright show-trace TestResults/Traces/SomeFailed_Test.zip
```

---

*Generated from `ApplicationRegistry.trx` · Run ID `a195aaba-b647-411e-b73a-782d69c62dd1`*
