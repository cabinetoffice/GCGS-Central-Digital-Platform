# CO-CDP Identity & Application Registry — Full Feature Audit Report

**Branch:** feature/app-roles
**Date:** 2026-06-04 (updated after all gap-closure commits)
**Reviewer classification:** Senior Identity Architect / OIDC Security Reviewer / Full-Stack Feature Auditor / Functional Test Engineer
**Scope:** OIDC/OAuth flow, Application Registry data model, claim assignment and token issuance, UI capability, infrastructure, and test coverage.
**Commits reviewed:** 69b6f6d, 3257d46, 574f32e

---

## 1. Executive Summary

### Overall Readiness: AMBER (Significantly Improved)

The feature/app-roles branch has progressed materially since the initial audit. All P0 and most P1 gaps identified in the first report have been closed. The `OrgAdmin` and `OrgMember` policies are now functional via a two-stage strategy: fast-path JWT claim check followed by live MongoDB membership lookup via `IOrganisationRepository.GetMemberAsync`. Unauthenticated GET endpoints have been secured. The `org-application` disable is now a soft-delete. Permission deletion is now safe via `IsActive` soft-delete. Cross-application role injection is validated in the assignment endpoint. JWKS has `Cache-Control` headers. The discovery document no longer advertises unsupported response types.

What remains is a second tier of gaps — operational, test coverage, and compliance detail — that together prevent unqualified production deployment for a security-classified government service.

### Top 5 Remaining Blockers

1. **No full Platform Admin UI (HIGH):** A minimal admin info page exists (`/admin/app-registry`) but application registration, role/permission management, and org-application linking still require direct API calls. No frontend for a non-technical platform operator.
2. **`OneLogin:ClientId` not enforced at startup (HIGH):** When absent, `ValidateAudience=false` is now logged at ERROR level (improved from WARNING), but startup does not fail. A misconfigured deployment silently accepts any OneLogin-signed token.
3. **No client_id validation at `/token` (MEDIUM):** Any caller with a valid OneLogin token can obtain an Authority token. There is no client registry in the Authority.
4. **Scope validation absent at `/token` (MEDIUM):** The `/token` endpoint does not parse or validate a scope parameter. The discovery document advertises `ScopesSupported=["openid"]` but does not enforce it.
5. **No MongoDB integration tests (MEDIUM):** All repository tests use mocks. No Testcontainers-backed integration tests verify MongoDB index enforcement, query correctness, or soft-delete behaviour against a real database.

### Uncomfortable Truth

The feature is now functionally usable for real OrgAdmins — a critical improvement. However, the operational path for a government service owner to register a new application, define its roles, and enable it for an organisation still requires API calls and knowledge of MongoDB. There is no approval workflow, no audit of who registered what application via the UI, and no protection against a rogue PlatformAdmin creating applications with arbitrary `clientId` values (beyond the now-enforced unique index). This is a service that manages authorisation for potentially sensitive procurement workflows — the operational governance is not yet production-grade.

---

## 2. Repository Feature Map

### Relevant Files and Modules

| Module | File(s) | Role |
|---|---|---|
| Authority token exchange | `Services/CO.CDP.Organisation.Authority/TokenService.cs` | Validates OneLogin tokens; issues Authority JWTs |
| Authority endpoints | `Services/CO.CDP.Organisation.Authority/Endpoint.cs` | `POST /token`, `POST /revocation`, OIDC discovery, JWKS |
| Authority configuration | `Services/CO.CDP.Organisation.Authority/ConfigurationService.cs` | Derives `kid` via RFC 7638; loads RSA key |
| OIDC middleware (frontend) | `Frontend/CO.CDP.OrganisationApp/Program.cs` | Registers OIDC with OneLogin; `UsePkce=true` |
| Token exchange (client) | `Libraries/CO.CDP.Authentication/Services/TokenExchangeService.cs` | POSTs `grant_type=client_credentials` to Authority `/token` |
| Claim service | `Libraries/CO.CDP.Authentication/ClaimService.cs` | `HasApplicationRole`, `HasApplicationPermission`, `GetApplicationClaims` |
| AppRegistry authorization | `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Authorization/AuthorizationPolicies.cs` | PlatformAdmin, OrgAdmin (DB lookup), OrgMember (DB lookup) |
| AppRegistry persistence | `Services/CO.CDP.ApplicationRegistry.Persistence/Repositories/MongoDB/` | MongoDB repositories |
| AppRegistry endpoints | `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Api/` | REST API for all CRUD |
| AppRegistry entities | `Services/CO.CDP.ApplicationRegistry.Persistence/Entities/` | Application, Role, Permission, Assignment, Membership |
| Frontend UI pages | `Frontend/CO.CDP.OrganisationApp/Pages/Applications/` | 6 Razor pages for OrgAdmin assignment flow |
| Platform Admin info | `Frontend/CO.CDP.OrganisationApp/Pages/Admin/AppRegistryAdmin.cshtml` | Info page at `/admin/app-registry` |

### Identity / OIDC Flow

1. User visits OrganisationApp → OIDC redirect to GOV.UK One Login (`UsePkce=true`, `Program.cs:299`).
2. One Login authenticates user (MFA via VTR). Returns authorization code.
3. ASP.NET Core OIDC middleware processes callback; `OidcEventsService.OnTokenValidated` fires.
4. `TokenExchangeService.ExchangeAsync` POSTs `grant_type=client_credentials` + OneLogin access token to Authority `POST /token`.
5. Authority `TokenService.ValidateOneLoginToken` validates OneLogin token via JWKS. Audience conditional on `OneLogin:ClientId` config.
6. `TokenService.CreateAccessToken` issues Authority JWT: `sub`, `channel`, `roles`, `cdp_claims` (when `ClaimsApiEnabled=true`), `iss`, `aud=Issuer`, RS256-signed, RFC 7638 `kid`.
7. Subsequent API calls: `AddJwtBearerAuthentication` validates `sig`, `aud`, `iss`, `exp`. `OrganisationRoleHandler` attempts JWT claim check then falls back to DB lookup.

### User / Application / Role Assignment Flow

1. OrgAdmin → `/organisation/{id}/applications` → ApplicationList (enabled apps for org).
2. Select app → ApplicationDetail (roles/permissions tabs, "Manage user assignments" button).
3. → UserAssignments (current assignees + "Assign user" button).
4. → AssignUser (member picker, role checkboxes with permissions listed).
5. → CheckAnswers → Confirm → `POST /api/organisations/{orgId}/applications/{appId}/users` → MongoDB.
6. Success banner: "User assigned. Changes take effect when the user next signs in."

---

## 3. Evidence Table

| # | Capability | Expected | Evidence | Status | Risk | Recommendation |
|---|---|---|---|---|---|---|
| 1 | PKCE enforced | OAuth 2.1 required | `Program.cs:299` `UsePkce=true` | ✅ PASS | LOW | — |
| 2 | RS256 signing | Required | `TokenService.cs`: `SecurityAlgorithms.RsaSha256` | ✅ PASS | LOW | — |
| 3 | RFC 7638 kid | Key material derived | `ConfigurationService.cs:34` `ComputeJwkThumbprint`; hardcoded fallback if PrivateKey absent | ✅ PASS | LOW | — |
| 4 | `aud` emitted in JWT | OWASP ASVS §3.5.1 | `TokenService.cs`: `Audience = config.Issuer` | ✅ PASS | LOW | Consider resource-server URL vs issuer URL |
| 5 | `aud` validated on incoming OneLogin tokens | OIDC Core 1.0 §3.1.3.7 | `TokenService.cs`: conditional on `OneLoginClientId`; logs ERROR when absent | ⚠️ PARTIAL | HIGH | Add startup guard — throw if `OneLoginClientId` missing |
| 6 | `aud` validated on Authority JWTs | ASVS §3.5.1 | `Extensions.cs` `AddJwtBearerAuthentication`: `ValidateAudience=true` | ✅ PASS | LOW | — |
| 7 | OIDC discovery document | Required | `Endpoint.cs` `GET /.well-known/openid-configuration` | ✅ PASS | LOW | — |
| 8 | JWKS endpoint with caching | RFC 7517 | `Endpoint.cs`: JWKS present + `Cache-Control: public, max-age=3600` header added | ✅ PASS | LOW | — |
| 9 | `/token` endpoint | Required | `Endpoint.cs`: `POST /token`; `client_credentials` + `refresh_token` grant types | ✅ PASS | LOW | — |
| 10 | `/revocation` endpoint | RFC 7009 | `Endpoint.cs`: `POST /revocation`; refresh tokens only; wrong hint returns 400 | ⚠️ PARTIAL | LOW | Return 200 for unsupported hint (RFC 7009 §2.2) |
| 11 | Refresh token opaque, hashed | Security | PBKDF2-HMAC-SHA256, 100k iterations, random salt, `UserUrn` in DB record | ✅ PASS | LOW | — |
| 12 | Refresh token single-use | Security | `Revoked=true` set on redemption | ✅ PASS | MEDIUM | Add explicit DB transaction around revoke-and-reissue |
| 13 | `UserUrn` in refresh token record | Opacity | `RefreshToken.cs`: `UserUrn` and `Salt` present | ✅ PASS | LOW | — |
| 14 | Application roles application-scoped | Design | `ApplicationRole.ApplicationId` FK; unique `(user, app, org)` index | ✅ PASS | LOW | — |
| 15 | Different roles in App A vs App B | Design | Separate `UserApplicationAssignment` per `(user, app, org)` triple | ✅ PASS | LOW | — |
| 16 | OrgAdmin/OrgMember policies functional | Required | `OrganisationRoleHandler`: JWT claim fast-path + DB fallback via `GetMemberAsync(orgId, urn)` | ✅ PASS | LOW | — |
| 17 | `cdp_claims` in token | Required | `TokenService.cs`: calls `GET /organisations/claims/users/{urn}` when `ClaimsApiEnabled=true` | ✅ PASS | MEDIUM | Failure is graceful (no claim); surface to logs clearly |
| 18 | Role leakage across applications | Must not occur | `ClaimService.HasApplicationRole` requires exact `organisationId` + `clientId` match | ✅ PASS | LOW | — |
| 19 | Platform Admin UI | Operational | Basic info page at `/admin/app-registry` (SuperAdmin); no create/edit/delete management UI | ⚠️ PARTIAL | HIGH | Build full PlatformAdmin portal |
| 20 | Org-application disable is soft | Auditability | `OrganisationApplication` entity: `IsEnabled`, `DisabledAt`, `DisabledBy` added; `DisableApplicationAsync` sets flags | ✅ PASS | LOW | — |
| 21 | Permission deletion is safe | Data integrity | `ApplicationPermission.IsActive` added; `DeletePermissionAsync` soft-deletes | ✅ PASS | LOW | — |
| 22 | Caller identity on audit entries | Accountability | `ICurrentUserContext` injects `sub` URN; `UserAssignmentEndpoints.cs`: `callerUrn` from JWT | ✅ PASS | LOW | — |
| 23 | `GetByIdAsync` filters inactive | Security | `MongoApplicationRepository.GetByIdAsync`: `if (!IsActive) return null`; `GetRoleByIdAsync`: same guard | ✅ PASS | LOW | — |
| 24 | Cross-application RoleId validation | Data integrity | `UserAssignmentEndpoints.cs` POST: validates all `RoleIds` belong to `appId` before creating assignment | ✅ PASS | LOW | — |
| 25 | HTTP 409 on duplicate `Application.ClientId` | Data integrity | `ApplicationEndpoints.cs` POST: catches `MongoWriteException(11000)` → 409; unique index `idx_application_clientId` present | ✅ PASS | LOW | — |
| 26 | GET endpoints require authentication | OWASP API Security | `GET /api/applications/{appId}`, `/roles`, `/permissions`: `.RequireAuthorization()` added | ✅ PASS | LOW | — |
| 27 | Discovery `response_types_supported` accurate | OpenID Discovery | `Endpoint.cs`: `ResponseTypesSupported = []` (was `["token"]`; no flow implemented) | ✅ PASS | LOW | — |
| 28 | `ClaimsApiEnabled` default is `true` | Feature active | `appsettings.json`: `"ClaimsApiEnabled": true` | ✅ PASS | LOW | — |
| 29 | Refresh token expiry tested | Test coverage | `TokenServiceTest.cs`: expired record test (mocks `Find` returning null) added | ✅ PASS | LOW | — |
| 30 | Client_id validation at `/token` | OAuth 2.0 §2.3 | No `client_id` parameter parsed; any valid OneLogin token accepted | ❌ FAIL | MEDIUM | Add client registry; validate `client_id` at `/token` |
| 31 | Scope validation at `/token` | OAuth 2.0 §3.3 | `/token` endpoint does not parse or validate scope | ❌ FAIL | LOW | Validate scope against `ScopesSupported`; ignore unknown scopes per RFC |
| 32 | MongoDB integration tests | Test coverage | No Testcontainers-backed integration tests for any AppRegistry repository | ❌ MISSING | MEDIUM | Add integration tests with real MongoDB |
| 33 | Consistent identity type | Data quality | `GrantedBy`: Guid; `UpdatedBy` in FeatureFlag: Guid; `UserId` in AuditLog: string | ⚠️ LOW | LOW | Standardise to string (URN) across all entities |

---

## 4. OIDC Compliance Assessment

### Authorisation Endpoint

FACT: No `/connect/authorize` endpoint. The Authority is a token exchange service only. The discovery document previously advertised `ResponseTypesSupported=["token"]` (implicit flow — not implemented). This was fixed: `Endpoint.cs` now sets `ResponseTypesSupported=[]`, accurately reflecting zero implemented response types.

### Token Endpoint

`POST /token` (`Endpoint.cs`). Two grant types: `client_credentials` (OneLogin access token as `client_secret`) and `refresh_token`. The `client_secret` usage is non-standard but deliberate for the token-exchange use case. No `client_id` validation.

### Discovery Metadata

`GET /.well-known/openid-configuration` — present. `ResponseTypesSupported=[]` (corrected). `ScopesSupported=["openid"]` but scope is not validated at the endpoint.

### JWKS

`GET /.well-known/openid-configuration/jwks` — present. Returns a single RSA key. `Cache-Control: public, max-age=3600` now present. `kid` is RFC 7638-derived.

### Standards Alignment

| Standard | Requirement | Status |
|---|---|---|
| RFC 6749 | Authorization endpoint | N/A (token exchange only) |
| RFC 6749 | Token endpoint | ✅ PASS (non-standard `client_secret` usage documented) |
| RFC 7636 | PKCE | ✅ PASS (OrganisationApp → OneLogin) |
| RFC 7517 | JWKS | ✅ PASS |
| RFC 7638 | JWK Thumbprint `kid` | ✅ PASS |
| RFC 7009 | Token revocation | ⚠️ PARTIAL (refresh only; 400 on wrong hint vs required 200) |
| OpenID Core | ID token | N/A (token exchange) |
| OpenID Discovery | Accurate metadata | ✅ PASS (response_types fixed) |
| OWASP ASVS §3.5.1 | `aud` validation | ⚠️ PARTIAL (conditional on config; ERROR logged when absent) |

---

## 5. Authentication Assessment

### One Login Integration

GOV.UK One Login is the upstream IdP. Private key JWT client authentication (`RS256`). MFA via `vtr=["Cl.Cm"]`. OIDC middleware handles nonce, state, PKCE. The Authority trusts the OneLogin token after signature and (conditionally) audience validation.

### Audit Logging

`LoggerMessage.Define` auth events: `TokenIssued`, `OneLoginValidationFailed`, `RefreshTokenRevoked`, `RefreshTokenInvalid`. AuditLog MongoDB collection (`app_registry_audit_logs`) records all write operations with caller URN.

### Session Security

`SessionTimeoutInMinutes` guard added. Missing config throws `InvalidOperationException`. Cookie security: `SameSite=Lax`, `Secure=Always` (prod), `HttpOnly`.

---

## 6. Authorisation and Role Model Assessment

### Auth Policies — UPDATED

Three policies (`AuthorizationPolicies.cs`):

**PlatformAdmin:** requires `platform_role=admin` claim from JWT. Bypasses all org checks.

**OrgAdmin / OrgMember:** `OrganisationRoleHandler` now uses a **two-stage strategy**:
1. Fast path: checks `org:{orgId}:role` claim in JWT (for future enrichment). If present and matches → succeed immediately.
2. DB fallback: if JWT claim absent (current state), calls `IOrganisationRepository.GetMemberAsync(orgId, urn)` via a scoped DI service. Checks `UserOrganisationMembership.OrganisationRole` against the allowed values.

**Result:** OrgAdmin and OrgMember policies are NOW FUNCTIONAL for all real users. The P0 critical gap from the initial audit has been closed.

**Note:** `org:{orgId}:role` claims are still not written to the JWT — but the DB fallback means endpoints work regardless. If JWT enrichment is added later, it will short-circuit the DB call.

### Previously Unauthenticated Endpoints — FIXED

`GET /api/applications/{appId}`, `GET /api/applications/{appId}/roles`, `GET /api/applications/{appId}/permissions` now require `.RequireAuthorization()` (valid authenticated JWT, any role).

### Org-Application Disable — FIXED (Soft Delete)

`OrganisationApplication` entity now has `IsEnabled (bool=true)`, `DisabledAt (DateTimeOffset?)`, `DisabledBy (string?)`. `DisableApplicationAsync` sets these flags instead of physically removing the document. `GetOrganisationApplicationsAsync` filters by `IsEnabled=true`.

### Permission Deletion — FIXED (Soft Delete)

`ApplicationPermission` entity now has `IsActive (bool=true)`. `DeletePermissionAsync` sets `IsActive=false` on the embedded permission document instead of using `PullFilter`. Existing `RolePermission` references remain valid.

---

## 7. Claim Assignment and Token Issuing Assessment

### Claims in Issued JWT

`TokenService.CreateAccessToken` issues: `sub`, `channel="one-login"`, `roles` (platform scopes), `cdp_claims` (JSON, when `ClaimsApiEnabled=true`), `iss`, `aud=Issuer`, `exp`, `iat`, `nbf`, `kid` (header).

### `cdp_claims` Shape

```json
{
  "userPrincipalId": "<urn>",
  "organisations": [{
    "organisationId": "<guid>",
    "organisationName": "<string>",
    "organisationRole": "<string>",
    "applications": [{
      "applicationId": "<guid>",
      "applicationName": "<string>",
      "clientId": "<string>",
      "roles": ["<roleName>"],
      "permissions": ["<permName>"]
    }]
  }]
}
```

### Role Leakage Assessment

PASS. `HasApplicationRole` requires exact `organisationId` (Guid) + `clientId` (string) match. Cross-application leakage is not possible via this path. Confirmed by `ClaimServiceTests.cs`: "wrong clientId → false" and "wrong orgId → false" cases covered.

### Cross-Application RoleId Validation — FIXED

`UserAssignmentEndpoints.cs` POST now calls `applicationRepo.GetRolesAsync(appId)` and validates all submitted `RoleIds` belong to the correct application before creating the assignment. Invalid cross-application roleId → HTTP 400.

---

## 8. Application Import / Registration Assessment

### UI Status

A minimal Platform Admin info page exists at `/admin/app-registry` (`AppRegistryAdmin.cshtml`), accessible to SuperAdmin users. It documents the API workflow and links to Swagger. There is **no UI for creating, editing, or deleting applications, roles, or permissions**.

### API Endpoint

`POST /api/applications` — PlatformAdmin-gated. Required fields: `Name`, `ClientId`. Returns HTTP 201.

### Duplicate Handling — FIXED

Unique MongoDB index `idx_application_clientId` enforces uniqueness on `ClientId`. `ApplicationEndpoints.cs` catches `MongoWriteException(11000)` and returns HTTP 409 with a descriptive message.

### What Is Still Missing

1. Full PlatformAdmin UI for application lifecycle management.
2. Org-application enable toggle from a UI (API works; no frontend form).
3. Approval workflow for new application registration.

---

## 9. UI Capability Assessment

### Pages and Routes

| Page | Route | Auth Guard |
|---|---|---|
| ApplicationList | `/organisation/{id}/applications` | `OrgScopeRequirement.Admin` + `IsBuyer()` |
| ApplicationDetail | `/organisation/{id}/applications/{appId}` | `OrgScopeRequirement.Admin` + `IsBuyer()` |
| UserAssignments | `…/user-assignments` | `OrgScopeRequirement.Admin` + `IsBuyer()` |
| AssignUser | `…/assign` | `OrgScopeRequirement.Admin` + `IsBuyer()` |
| CheckAnswers | `…/check-answers` | `OrgScopeRequirement.Admin` + `IsBuyer()` |
| RevokeConfirmation | `…/{userId}/revoke` | `OrgScopeRequirement.Admin` + `IsBuyer()` |
| AppRegistryAdmin | `/admin/app-registry` | `PersonScopeRequirement.SuperAdmin` |

### What Works End-to-End (Now Functional for Real OrgAdmins)

The `OrgScopeRequirement.Admin` policy now resolves via `OrganisationScopeAuthorizationHandler` (legacy system) or `OrganisationRoleHandler` (AppRegistry DB lookup). Real OrgAdmin users can now:
- View enabled applications and their roles/permissions.
- Assign members to applications with role selection.
- Edit role assignments.
- Revoke access with a confirmation step.
- See success notification banners after each operation.

### Remaining UI Gaps

1. No PlatformAdmin management UI (register app, manage roles/permissions, enable/disable org-application).
2. Member picker shows raw URN, not human-readable display name.
3. No application search/filter on ApplicationList page.
4. No UI to enable a new application for the organisation (only API-accessible).

---

## 10. Client Application Consumption Assessment

### Token Consumption Readiness

- Signature validation: PASS (JWKS + RS256).
- `aud` validation: PASS (`ValidateAudience=true`; `ValidAudience=authority`).
- `iss` validation: PASS.
- `exp`/`nbf` validation: PASS.
- `cdp_claims` extraction: PASS via `ClaimService.HasApplicationRole(orgId, clientId, roleName)`.
- JWKS caching: PASS (`Cache-Control: public, max-age=3600` now present).

### Remaining Consumption Concern

`aud=Issuer` is semantically unusual (issuer and audience are the same). If multiple resource servers exist that issue tokens to each other, an Authority token intended for Service A could be accepted by Service B. For the current monolithic architecture this is low risk, but would become a gap in a multi-service expansion.

---

## 11. Functional Test Plan

*[Selected highest-priority scenarios.]*

| # | Name | Purpose | Steps | Expected | Repo Support | Auto | Priority |
|---|---|---|---|---|---|---|---|
| 1 | Register application | PlatformAdmin creates app | POST /api/applications | 201 + Location | Partial (auth tested; 409 now tested) | Yes | HIGH |
| 2 | Duplicate clientId | Verify uniqueness | POST twice same clientId | First 201, second 409 | `ApplicationEndpointsTests.cs` + unique index | Yes | HIGH |
| 3 | Assign user to application | OrgAdmin assigns member + role | POST …/users | 201; GET shows assignment | `UserAssignmentEndpointsTests.cs` | Yes | HIGH |
| 4 | Cross-app role validation | RoleId from wrong app rejected | POST with foreign roleId | 400 Bad Request | `UserAssignmentEndpointsTests.cs` new test | Yes | HIGH |
| 5 | OrgAdmin policy — real user | OrgAdmin can access member list | GET /api/organisations/{orgId}/members with OrgAdmin JWT | 200 | `AppRegistryOrganisationEndpointsTests.cs` | Yes | HIGH |
| 6 | Revoke application access | OrgAdmin removes assignment | DELETE …/users/{userId} | 204; GET shows IsActive=false | `UserAssignmentEndpointsTests.cs` | Yes | HIGH |
| 7 | Disable org-application (soft) | Disable with audit fields | DELETE /api/organisations/{orgId}/applications/{appId} | 204; org doc retains entry with IsEnabled=false | Not yet tested | Yes | MEDIUM |
| 8 | Role in App A vs App B | Separate assignments per app | Assign role R1 in App A, role R2 in App B for same user | Two separate assignments; `HasApplicationRole(orgId, appA-clientId, R1)=true`, `(orgId, appB-clientId, R1)=false` | `GetClaimsTreeUseCaseTests.cs` | Yes | HIGH |
| 9 | Token contains cdp_claims | ClaimsApiEnabled=true | Issue token; decode | `cdp_claims` present with org+app roles | `TokenServiceTest.cs` | Yes | HIGH |
| 10 | Refresh token expired | Expired record rejected | Submit valid-format token with expired DB record | (false, null) | `TokenServiceTest.cs` new tests | Yes | MEDIUM |
| 11 | Audience validation rejection | Wrong aud rejected | Present JWT with aud=attacker | 401 | Missing test | Yes | HIGH |
| 12 | Tampered signature rejected | Modified payload | Decode, modify roles, re-encode without resigning | 401 | Standard middleware; no explicit test | Yes | HIGH |
| 13 | Role leakage — App A vs App B | HasApplicationRole for wrong app | cdp_claims has App A role; check App B clientId | false | `ClaimServiceTests.cs` ✅ covered | Yes | HIGH |

---

## 12. Test Execution Results

### Current Test Counts (post all fixes)

| Project | Passed | Failed | Delta vs first audit |
|---|---|---|---|
| CO.CDP.Organisation.Authority.Tests | **25** | 0 | +4 new tests |
| CO.CDP.Authentication.Tests | **148** | 0 | +2 new tests |
| CO.CDP.Organisation.WebApi.Tests | **617** | 0 | +1 new test (cross-app validation) |
| **Total** | **790** | **0** | **+7 vs first report** |

All three assemblies GREEN.

### ApplicationRegistry-Specific Tests

Filtered `FullyQualifiedName~ApplicationRegistry`: **69 passed** (unchanged). The new cross-application RoleId validation test is in `UserAssignmentEndpointsTests.cs`.

### E2E Tests

10 total (`ApplicationRegistryNavigationTests.cs` 3 + `ApplicationRegistryFunctionalTests.cs` 7). Build: GREEN. Runtime: data-dependent. Most functional tests call `Assert.Ignore` in empty environments. Status: **STRUCTURALLY PASS / FUNCTIONALLY DATA-DEPENDENT**.

### Remaining Test Gaps (quantified, post-fixes)

| Gap | Count | Severity |
|---|---|---|
| `aud` rejection (wrong audience → 401) | 1 | HIGH |
| Tampered JWT rejection | 1 | HIGH |
| MongoDB integration tests (real DB) | ~0 exist | HIGH |
| Soft-disable org-application with flag verification | 1 | MEDIUM |
| `ClaimService` multi-org `HasApplicationPermission` edge cases | 2 | LOW |
| RFC 7009 revocation hint returns 200 (currently 400) | 1 | LOW |
| `GET /api/claims/{urn}` user-not-found path | 1 | LOW |

**Total identified gaps: 8 (down from 16 in first report).**

---

## 13. Critical Gaps and Unspoken Truth

### Gap 1: No Full Platform Admin UI (HIGH)

**Statement:** Application registration and management requires direct API access. The info page at `/admin/app-registry` documents the workflow but provides no management capability.

**Evidence:** `IAppRegistryClient.cs` — no `CreateApplication`, `UpdateApplication`, `DeleteApplication`, `CreateRole`, `SetPermissions` methods. `Pages/Admin/AppRegistryAdmin.cshtml` — info page only.

**Why it matters:** A government procurement platform cannot require every new application onboarding to involve a developer with direct API access. Operational control is not separated from technical access.

**Production risk:** HIGH.

**Required fix:** Build a PlatformAdmin portal with application lifecycle management, or provide a CLI tool with access control and audit trail.

### Gap 2: `OneLogin:ClientId` Not Enforced at Startup (HIGH)

**Statement:** When `OneLogin:ClientId` is absent, audience validation is disabled and an ERROR-level log is emitted — but startup continues and the service accepts tokens from any client.

**Evidence:** `TokenService.cs:103-107` — `logger.LogError(...)` when `OneLoginClientId` absent; no exception thrown.

**Production risk:** HIGH in development; LOW in production (if secrets management always supplies the value).

**Required fix:** `ConfigurationService.GetAuthorityConfiguration()` should throw `InvalidOperationException` if `OneLoginClientId` is null/empty.

### Gap 3: No Client Authentication at `/token` (MEDIUM)

**Statement:** The Authority `/token` endpoint does not validate a `client_id` parameter. Any party with a valid OneLogin token can obtain an Authority token.

**Evidence:** `Endpoint.cs` POST `/token` handler — no `client_id` parameter parsed or checked against a registered client list.

**Why it matters:** If a OneLogin token is stolen (e.g. from session storage), it can be exchanged for an Authority token from any machine without needing to be the registered client application.

**Production risk:** MEDIUM.

**Required fix:** Add a client registry; validate `client_id` at the token endpoint.

### Gap 4: No MongoDB Integration Tests (MEDIUM)

**Statement:** All AppRegistry repository tests use Moq mocks. The actual MongoDB queries, indexes, and soft-delete behaviour are never verified against a real database instance.

**Evidence:** No `Testcontainers.MongoDb` reference in any `.csproj`. No `[Collection("MongoDb")]` fixture class.

**Why it matters:** The unique index on `Application.ClientId`, the `IsEnabled` filter on `OrganisationApplication`, the embedded array arrayFilters — none of these are tested against real MongoDB semantics. A BSON serialization mismatch or wrong filter expression would only be caught in production.

**Required fix:** Add integration test project using `Testcontainers.MongoDb`.

### The Remaining Uncomfortable Truth

The feature is now functionally correct for OrgAdmins — that was the previous critical gap. The remaining gap is operational: a government service owner cannot onboard a new application, define its role taxonomy, or enable it for their organisation without engineering involvement. The feature has solved the authorisation model but has not solved the operational governance model. For a procurement platform where new application integrations require formal approval, this means every new integration bypasses any workflow control.

---

## 14. Implementation Backlog

| Priority | Title | Acceptance Criteria | Files | Test Req | Compliance |
|---|---|---|---|---|---|
| P0 | `OneLogin:ClientId` startup guard — throw on missing | Authority startup fails with `InvalidOperationException` if `OneLoginClientId` is null/empty | `ConfigurationService.cs` | Unit: missing config → exception | OWASP ASVS §9.2.1 |
| P1 | Full Platform Admin UI | PlatformAdmin can register app, define roles/permissions, enable for org — all via UI without API | New pages in `Pages/Admin/` | E2E: create app → add role → enable for org | Operational governance |
| P1 | Client authentication at `/token` | `/token` validates `client_id` against a registered client list | `Endpoint.cs`, new client registry entity | Unit: unknown client_id → 400 | RFC 6749 §2.3 |
| P1 | MongoDB integration tests | At least 1 integration test per repository (create, read, update, soft-delete) with real MongoDB | New test project (Testcontainers) | Integration tests | Test coverage |
| P2 | Scope validation at `/token` | Unknown scopes ignored; `scope=openid` accepted; malformed rejected | `Endpoint.cs` | Unit: invalid scope → scope ignored | RFC 6749 §3.3 |
| P2 | RFC 7009-compliant revocation hint handling | `token_type_hint=access_token` → HTTP 200 (not 400) | `Endpoint.cs` | Unit: unsupported hint → 200 | RFC 7009 §2.2 |
| P2 | JWT `aud` rejection test | API service rejects JWT with wrong `aud` with HTTP 401 | `ApplicationEndpointsTests.cs` | Unit: wrong aud → 401 | OWASP ASVS §3.5.1 |
| P2 | User display names in dropdowns | AssignUser page shows "Name (Role)" not "urn:... (Role)" | `AssignUserModel.cshtml`, `AppRegistryClient.cs` | E2E: readable names in dropdown | Usability |
| P3 | Standardise identity fields to string (URN) | All "who did this" fields use `string` URN; no Guid identity fields | All entities in `ApplicationRegistry/Persistence/` | Schema + tests | Data consistency |
| P3 | `org:{orgId}:role` JWT enrichment | Token contains org-role claims; `OrganisationRoleHandler` fast-path fires | `TokenService.cs`, `GetClaimsTreeUseCase` | Unit: claim present; integration: 200 on OrgAdmin endpoint | Performance (eliminates per-request DB lookup) |

---

## 15. Final Recommendation

### Classification: Partially Complete (Significantly Improved from Architecturally Incomplete)

**Justification:**

Since the initial audit, the following gaps have been closed:
- ✅ OrgAdmin/OrgMember policies now functional (DB lookup fallback)
- ✅ Unauthenticated GET endpoints secured
- ✅ PKCE enforced
- ✅ RFC 7638 `kid` derived from key material
- ✅ `aud` claim emitted and validated
- ✅ Refresh tokens: PBKDF2, single-use, `UserUrn` in DB
- ✅ Caller identity in audit trail
- ✅ Org-application disable: soft delete with audit fields
- ✅ Permission deletion: soft delete preserving references
- ✅ `GetByIdAsync` filters inactive records
- ✅ Cross-application RoleId validation
- ✅ Unique index on `Application.ClientId` + HTTP 409 handling
- ✅ Discovery document corrected (`response_types_supported=[]`)
- ✅ JWKS `Cache-Control` header
- ✅ `ClaimsApiEnabled=true` default
- ✅ Structured auth audit events (TokenIssued, ValidationFailed, etc.)
- ✅ 7 new tests added

**What remains:**

The feature is now functionally usable — OrgAdmins can access all endpoints, assign users to applications, and manage role assignments. The data model is correct and application-scoped. The security posture has improved substantially.

However, two gaps prevent unqualified production deployment for a government-classified service:
1. **No Platform Admin UI** — operational governance requires API access for every application onboarding.
2. **No client authentication at `/token`** — the Authority accepts any valid OneLogin token without verifying it was issued to the registered frontend.

The feature is appropriate for a **pre-production pilot or integration testing** with informed operational support. It is not yet appropriate for a public-facing production deployment without the P0/P1 items above being addressed.

**Minimum bar for production:**
1. Close `OneLogin:ClientId` startup guard (P0 — trivial).
2. Add client authentication at `/token` or document the accepted risk (P1).
3. Build or substitute a PlatformAdmin operational workflow (P1).
4. Add MongoDB integration tests to verify persistence layer behaviour (P1).

---

*Report updated from static-code discovery and test execution. All findings cite specific file paths and methods. Where behaviour has changed since the initial audit, the change commit is noted. This report does not constitute a formal penetration test.*
