# Identity and Application Roles Security Review

**Project:** CO-CDP (Crown Commercial Service / Cabinet Office Central Digital Platform)
**Branch:** feature/app-roles
**Reviewer:** Senior Identity Architect and Security Auditor
**Date:** 2026-06-03
**Classification:** INTERNAL — RESTRICTED

---

## 1. Executive Summary

**Readiness Rating: RED**

The feature/app-roles branch introduces a materially significant Application Registry sub-system and extends the Authority token service to carry application-scoped roles via the `cdp_claims` JWT claim. However, several foundational security controls are absent or incomplete in ways that would cause an NCSC or GDS security review to fail outright. The Authority service disables audience validation for incoming GOV.UK One Login tokens, meaning any validly signed JWT from the issuer is accepted regardless of intended audience. The application-role feature flag (`Features:ClaimsApiEnabled`) defaults to `false` in `appsettings.json`, so application roles are absent from every token issued in the default and local configurations. The Application Registry's enable/disable endpoints for organisation-application linkage are stubs that persist no data. The audit trail across all write operations records a hardcoded literal `"system"` instead of the authenticated caller's identity, producing zero real-actor accountability. PKCE is suppressed in the OrganisationApp OIDC client. Taken together, the system cannot be considered production-ready for any security-classified government service.

**Top 5 Blockers**

1. **Audience validation disabled** (`ValidateAudience = false`) in `TokenService.ValidateInternalAsync` and `Extensions.AddJwtBearerAuthentication` — any valid-signature JWT from the correct issuer is accepted at the token exchange endpoint and at all downstream API services, regardless of intended audience. Violates OIDC Core 1.0 §3.1.3.7 and OWASP ASVS v4 §3.5.1.
2. **`cdp_claims` feature flag off by default** — `Features:ClaimsApiEnabled=false` in `appsettings.json` means application roles are never written to issued tokens in any environment that does not explicitly override this flag. The entire application-role authorisation model is silently absent.
3. **Enable/disable application assignment endpoints are stubs** — `POST /api/organisations/{orgId}/applications/{appId}` and `DELETE /api/organisations/{orgId}/applications/{appId}` return HTTP 201/204 with no repository call in `OrganisationEndpoints.cs` lines 146–162, making it impossible to grant or revoke applications for organisations through the API.
4. **No real-actor audit trail** — `AuditLog.UserId` is hardcoded to `"system"` in all three MongoDB repositories, and `AssignedBy` is hardcoded to `"system"` in `UserAssignmentEndpoints.cs` line 43. No authenticated caller identity is captured at any write point.
5. **Refresh token URN leakage** — the PBKDF2 salt IS the UTF-8-encoded URN, encoded as Base64 and concatenated into the opaque refresh token string. Any party holding the raw token can recover the user's URN without database access, violating the principle that opaque tokens should carry no meaningful information.

---

## 2. Feature Architecture Map

The following describes the end-to-end login-to-role-check flow as evidenced by the codebase. File references are given at each step.

```
Step 1 — Browser initiates OIDC login
  OrganisationApp OidcEvents.RedirectToIdentityProvider()
  File: /Services/CO.CDP.Authentication/OidcEvents.cs (or OrganisationApp OidcEvents)
  - Adds vtr=["Cl.Cm"] (MFA required), ui_locales=en to redirect
  - UsePkce = false (OrganisationApp) / true (RegisterOfCommercialTools)

Step 2 — GOV.UK One Login authenticates user
  External: {OneLogin:Authority}
  - Handles authorization code flow
  - Returns id_token signed by OneLogin RSA key, plus authorization code

Step 3 — OIDC middleware exchanges code, receives id_token
  ASP.NET Core OIDC middleware (built-in)
  File: /Services/CO.CDP.Authentication/AuthenticationExtensions.cs (AddOneLoginAuthentication)
  - NonceCookie and CorrelationCookie validated by middleware
  - SaveTokens = true: id_token stored in auth ticket

Step 4 — OrganisationApp calls Authority POST /token
  File: /Frontend/CO.CDP.OrganisationApp/Services/TokenExchangeService.cs
  - grant_type=client_credentials
  - client_secret = OneLogin access_token (form-encoded)
  - POST to {Authority}/token

Step 5 — Authority validates OneLogin token
  File: /Services/CO.CDP.Organisation.Authority/TokenService.cs (ValidateOneLoginToken → ValidateInternalAsync)
  File: /Services/CO.CDP.Organisation.Authority/ConfigurationService.cs (GetOneLoginConfiguration)
  - Fetches OneLogin JWKS from {OneLogin:Authority}/.well-known/openid-configuration
  - ValidateAudience = false [SECURITY GAP]
  - On SecurityTokenSignatureKeyNotFoundException: retries once with refresh=true

Step 6 — Authority resolves Person and issues tokens
  File: /Services/CO.CDP.Organisation.Authority/TokenService.cs (CreateToken, CreateAccessToken, CreateRefreshToken)
  - Resolves Person via IPersonRepository.FindByUrn(urn)
  - Calls GET /organisations/claims/users/{urn} via OrganisationApiHttpClient [only if Features:ClaimsApiEnabled=true]
  - Builds JWT: sub, channel, roles (person.Scopes), optionally cdp_claims
  - Signed RS256, kid = hardcoded constant "c2c3b22ac07f425eb893123de395464e"
  - No Audience field in SecurityTokenDescriptor [SECURITY GAP]
  - Access token: 3600s expiry (hardcoded)
  - Refresh token: PBKDF2 hash; salt = UTF8(URN) → Base64 [SECURITY GAP]
  - Stored in Postgres via IAuthorityRepository

Step 7 — cdp_claims assembly (when ClaimsApiEnabled=true)
  File: /Services/CO.CDP.Organisation.WebApi/Api/ApplicationRegistry/ClaimsEndpoints.cs
  File: /Services/CO.CDP.Organisation.WebApi/UseCase/ApplicationRegistry/GetUserClaimsUseCase.cs
  - GET /organisations/claims/users/{urn}
  - Fetches UserApplicationAssignments + roles + permissions from MongoDB
  - Returns JSON: {userPrincipalId, organisations:[{organisationId, organisationName, organisationRole, applications:[{applicationId, applicationName, clientId, roles, permissions}]}]}
  - Embedded as cdp_claims in JWT (JsonClaimValueTypes.Json)

Step 8 — Authority tokens stored in distributed session (Redis)
  File: /Frontend/CO.CDP.OrganisationApp/Services/TokenExchangeService.cs
  File: /Frontend/CO.CDP.OrganisationApp/Services/AuthorityTokenSet.cs
  - AuthorityTokenSet stored under key "UserAuthTokens" in Redis-backed IDistributedSession
  - 30-second client-side expiry buffer applied

Step 9 — API service validates Authority JWT
  File: /Libraries/CO.CDP.Authentication/AuthenticationExtensions.cs (AddJwtBearerAuthentication)
  - ValidateAudience = false [SECURITY GAP]
  - ValidateIssuer = true, ValidIssuer = configured Authority URL
  - Signature validated against JWKS at /.well-known/openid-configuration/jwks

Step 10 — Application-role authorisation checked
  File: /Libraries/CO.CDP.Authentication/ClaimService.cs
  File: /Libraries/CO.CDP.Authentication/AuthorizationExtensions.cs
  - ClaimService.HasApplicationRole(orgId, clientId, roleName) reads cdp_claims JSON
  - ClaimService.HasApplicationPermission(orgId, clientId, permissionName) reads cdp_claims JSON
  - ApplicationScopeAuthorizationHandler enforces these at policy level

Step 11 — Logout and revocation
  File: /Frontend/CO.CDP.OrganisationApp/Services/LogoutManager.cs
  - Writes Redis key "LoggedOutUser_{urn}" with TTL = SessionTimeoutInMinutes
  - Does NOT call POST /revocation directly
  File: /Services/CO.CDP.Organisation.Authority/Endpoint.cs (POST /revocation)
  - Only refresh_token supported; access_token revocation returns 400
```

---

## 3. Evidence Table

| # | Capability | Expected (OIDC/OAuth/ASVS Standard) | Evidence (File and Method) | Status | Risk | Recommendation |
|---|---|---|---|---|---|---|
| 1 | Audience validation on incoming OneLogin tokens | `ValidateAudience=true`, aud=clientId (OIDC Core 1.0 §3.1.3.7) | `TokenService.ValidateInternalAsync` — `ValidateAudience = false`. File: `TokenService.cs` | FAIL | HIGH: Any valid-signature JWT from OneLogin issuer accepted regardless of intended client | Set `ValidateAudience=true`, `ValidAudiences=[clientId]` in `ValidateInternalAsync` |
| 2 | Audience validation on API services | `ValidateAudience=true`, aud=service identifier (ASVS §3.5.1) | `Extensions.AddJwtBearerAuthentication` — `ValidateAudience = false`. File: `AuthenticationExtensions.cs` | FAIL | HIGH: Authority JWTs accepted at any service regardless of intended audience | Set audience on `SecurityTokenDescriptor` in `TokenService.CreateAccessToken`; enable validation in `AddJwtBearerAuthentication` |
| 3 | Application roles in JWT | cdp_claims (or equivalent) populated for all tokens | `Features:ClaimsApiEnabled=false` in `appsettings.json`. cdp_claims only written when flag is true. File: `appsettings.json`, `TokenService.CreateToken` | FAIL | CRITICAL: Application roles absent from all tokens in default config | Set `Features:ClaimsApiEnabled=true` as the production default; remove the feature flag gate once stable |
| 4 | Enable/disable org-application linkage | POST/DELETE persist data to MongoDB | `OrganisationEndpoints.cs` lines 146–162: `return Results.Created()` and `return Results.NoContent()` — no repo call | FAIL | HIGH: Applications cannot be granted to or revoked from organisations via the API | Implement `IOrganisationApplicationRepository` calls in both stub handlers |
| 5 | Real-actor audit trail | `AuditLog.UserId` = authenticated caller identity | `MongoApplicationRepository`, `MongoUserAssignmentRepository`, `MongoOrganisationRepository` all write `UserId = "system"`. `UserAssignmentEndpoints.cs` line 43: `AssignedBy = "system"` | FAIL | HIGH: Zero accountability for who performed any write operation | Extract authenticated principal from `IHttpContextAccessor`; pass caller URN to all repository write methods |
| 6 | Refresh token opacity | Token string contains no decodable meaningful data (RFC 6749 §10.10) | `TokenService.CreateRefreshToken` — salt = `Convert.ToBase64String(Encoding.UTF8.GetBytes(urn))`. URN recoverable by Base64-decoding the first token segment. File: `TokenService.cs` | FAIL | MEDIUM: URN embedded in opaque token; user identity recoverable without DB access | Use a random 256-bit salt unrelated to URN; store URN separately in the RefreshToken database record |
| 7 | Static key identifier (kid) | kid derived from key material or dynamic (RFC 7517 §4.5) | `AuthorityConfiguration.cs` — kid is hardcoded constant string `"c2c3b22ac07f425eb893123de395464e"`. Not derived from key material. File: `AuthorityConfiguration.cs` | FAIL | MEDIUM: Key rotation does not update kid; relying parties cannot distinguish key versions after rotation | Derive kid as SHA-256 thumbprint of the RSA public key (RFC 7638); regenerate automatically on key load |
| 8 | PKCE for OrganisationApp | `UsePkce=true` for all OIDC clients (OAuth 2.1, FAPI 1.0) | OrganisationApp `OidcOptions.UsePkce = false`. RegisterOfCommercialTools correctly sets `UsePkce=true`. Files: OrganisationApp OIDC configuration | FAIL | MEDIUM: OrganisationApp authorization code flow lacks PKCE protection | Enable `UsePkce=true` in OrganisationApp OIDC configuration; note private_key_jwt already mitigates the primary risk |
| 9 | Structured auth audit log | Structured events for login, token issuance, refresh, revocation (NCSC Cloud Security Principle 13) | No structured audit events in Authority service. `AuditLog` entity exists in AppRegistry persistence only. File: `TokenService.cs` — only `LogInformation`/`LogError` unstructured calls | FAIL | HIGH: No forensic record of authentication events | Implement structured `ILogger<TokenService>` events with structured properties: `{Event, URN, GrantType, Success, Timestamp}` at minimum |
| 10 | Client validation at POST /token | Client authentication / client_id allow-list (OAuth 2.0 §2.3) | `Endpoint.cs` POST /token — no `client_id` parameter parsed or validated. Any caller with a valid OneLogin token can obtain an Authority token | FAIL | MEDIUM: No client concept at the token endpoint; cannot restrict token issuance to registered frontends | Register known client_ids; validate `client_id` parameter against allow-list at POST /token |
| 11 | Access token revocation | Immediate invalidation on logout (ASVS §3.3.2) | POST /revocation only supports `refresh_token`; access token revocation returns HTTP 400. Logout marks Redis key but does not invalidate in-flight access tokens. File: `Endpoint.cs` | PARTIAL | MEDIUM: Stolen access tokens valid for up to 1 hour after logout | Implement token binding or shorter access token expiry; consider a revocation list for high-value operations |
| 12 | Configurable token expiry | Expiry configurable per environment | `TokenService.CreateToken` — access=3600s, refresh=86400s hardcoded constants. Not in appsettings. File: `TokenService.cs` | FAIL | LOW: Cannot tighten expiry without code change | Move expiry values to `AuthorityConfiguration` and load from appsettings |
| 13 | Application registration UI | UI for registering applications into the registry | No CreateApplication or RegisterApplication page exists. `IAppRegistryClient` has no CreateApplication method. Files: all files in `CO.CDP.OrganisationApp/Pages/Applications/` | FAIL | MEDIUM: Application registration is dev/API-only; no operational path for production onboarding | Implement a PlatformAdmin application registration page |
| 14 | MFA enforcement | MFA requested via VTR | `vtr=["Cl.Cm"]` added in all three OIDC event handlers. MFA enforced by OneLogin upstream. Files: OidcEvents, OidcEventsService | PASS | — | No action required |
| 15 | SameSite / Secure cookie policy | SameSite=Strict or Lax + Secure for all session cookies | Auth cookie SameSite=Lax, Secure=Always (prod). Nonce/Correlation cookies Secure+HttpOnly. Dev uses SameAsRequest. Files: `AuthenticationExtensions.cs` | PASS (prod) | LOW (dev only): SameAsRequest in dev | Acceptable for dev; verify prod pipeline sets the correct policy |
| 16 | Key rotation resilience | JWKS retry on `SecurityTokenSignatureKeyNotFoundException` | `TokenService.ValidateInternalAsync` retries once with `refresh=true` on key exception. File: `TokenService.cs` | PASS | — | No action required |
| 17 | IsActive filtering on assignments | Inactive assignments excluded from authorisation | `GetAssignmentsAsync`, `GetRolesAsync`, `GetAllAsync`, `GetMembersAsync` all filter `IsActive`. Files: MongoDB repository classes | PASS | — | No action required |
| 18 | SUPERADMIN role enforcement | SUPERADMIN-gated pages for platform management | `PersonScopes.SUPERADMIN` constant defined. No pages in Frontend use it. File: `AuthorizationExtensions.cs`, `CO.CDP.OrganisationApp/Pages/` | FAIL | MEDIUM: SUPERADMIN scope is a dead constant with no enforcement surface | Implement PlatformAdmin pages guarded by `PersonScopeRequirement.SuperAdmin` |
| 19 | `org` claim in JWT | ClaimType.OrganisationId readable from token | `ClaimService.GetOrganisationId()` reads `org` claim; `TokenService.CreateAccessToken` never writes it. File: `TokenService.cs`, `ClaimService.cs` | FAIL | LOW: `GetOrganisationId()` always returns null/default | Either write `org` claim in `CreateAccessToken` or remove `ClaimService.GetOrganisationId()` |
| 20 | Session timeout configurability | Explicit `SessionTimeoutInMinutes` in all environments | `LogoutManager` calls `GetValue<double>("SessionTimeoutInMinutes")` — zero default if missing; would cause incorrect Redis TTL. File: `LogoutManager.cs` | PARTIAL | LOW: Missing config silently produces a zero TTL, not an explicit error | Add `GetValue<double>(...) is 0 ? throw new InvalidOperationException(...)` guard |
| 21 | Audience scoping of application roles | Roles scoped to specific resource server / audience | No `aud` claim emitted by `CreateAccessToken`; no per-audience filtering of cdp_claims at issuance. File: `TokenService.cs` | FAIL | MEDIUM: All application roles for all organisations embedded in every token regardless of calling service | Emit `aud` claim; filter cdp_claims to roles relevant to the calling service or accept the current risk with documented rationale |
| 22 | Duplicate user assignment handling | HTTP 409 on duplicate assignment attempt | No conflict-handling middleware; MongoDB unique index `idx_userassignment_unique` would throw, resulting in HTTP 500. File: `UserAssignmentEndpoints.cs` | FAIL | LOW: Unexpected 500 surfaces internal error to caller | Catch `MongoDuplicateKeyException` in endpoint handler; return HTTP 409 with descriptive body |
| 23 | AccessControlEntry active-only filter | Revoked ACEs excluded from authorisation checks | `AccessControlEntry` has nullable `RevokedAt` but no `IsActive`; `GetAclEntriesAsync` interface does not filter revoked entries. No repository evidence of active-only filter. | FAIL | MEDIUM: Revoked access control entries may remain in effect | Add `IsActive` flag to `AccessControlEntry`; enforce active-only filter in `GetAclEntriesAsync` implementation |
| 24 | End-to-end integrated test | Login → token → cdp_claims → role-check tested as a unit | No integrated test exists. `TokenServiceTest` and `ClaimServiceTests` are isolated. E2E tests conditionally skipped (Assert.Ignore). Files: `TokenServiceTest.cs`, `ClaimServiceTests.cs`, `ApplicationRegistryFunctionalTests.cs` | FAIL | HIGH: The core security flow is never exercised as a whole | Implement integration test wiring real JWT → `ValidateOneLoginToken` → `CreateToken` → `ClaimService.HasApplicationRole` |

---

## 4. OIDC Flow Assessment

### 4.1 Authentication (GOV.UK One Login)

**FACT:** All three OIDC clients (OrganisationApp, RegisterOfCommercialTools, and the shared Authentication library) add `vtr=["Cl.Cm"]` to every redirect, requesting medium confidence authentication including MFA via the GOV.UK One Login standard. Client authentication to OneLogin uses `private_key_jwt` with RS256. The client assertion is a short-lived JWT (5-minute expiry) with `jti=Guid.NewGuid()` to prevent replay.

**FACT:** OrganisationApp sets `UsePkce = false`. RegisterOfCommercialTools sets `UsePkce = true`. The nonce is handled by ASP.NET Core OIDC middleware internally. The correlation and nonce cookies are correctly configured with SameSite=Lax, Secure, HttpOnly.

**INFERENCE:** PKCE was likely suppressed in OrganisationApp because `private_key_jwt` is already used as a strong client authentication mechanism. While this partially mitigates the primary PKCE threat model (authorization code interception without client authentication), OAuth 2.1 mandates PKCE for all authorization code flows regardless of client authentication method. This is a standards non-compliance finding.

**RISK:** An attacker who intercepts an authorization code issued to OrganisationApp has a window to exchange it (limited by the absence of PKCE binding). The practical risk is reduced by `private_key_jwt` but not eliminated.

**OPINION:** Enable PKCE for OrganisationApp. The overhead is negligible; the standards alignment benefit is significant for a government service that may be subject to NCSC assessment.

### 4.2 Authorisation (After Login)

**FACT:** The Authority service does not implement a `/connect/authorize` endpoint. It is a token exchange service, not a full OAuth authorization server. Authorization code flow and user authentication are delegated entirely to OneLogin.

**FACT:** After login, the frontend calls `POST /token` on the Authority with `grant_type=client_credentials` and `client_secret = oneLoginAccessToken`. No `client_id` parameter is validated. Any caller presenting a valid OneLogin token can obtain an Authority token.

**RISK:** Without client validation at the token endpoint, a compromised OneLogin token (obtained by any means) can be exchanged for an Authority token from any machine or service — not just the registered frontends. The token endpoint is effectively open to all authenticated users.

### 4.3 Claim Assembly

**FACT:** The Authority token always includes `sub` (URN), `channel` ("one-login"), and `roles` (person.Scopes CSV from Postgres). The `cdp_claims` claim is conditionally added only when `Features:ClaimsApiEnabled=true` AND the HTTP call to `GET /organisations/claims/users/{urn}` returns HTTP 200.

**FACT:** `Features:ClaimsApiEnabled` defaults to `false` in `appsettings.json`. Application roles are absent from all tokens in any environment that does not explicitly override this flag.

**FACT:** The `cdp_claims` JSON embeds ALL application roles for ALL organisations the user belongs to, across all applications. There is no per-resource or per-audience filtering.

**INFERENCE:** The claim assembly is structurally correct for the first iteration, but the missing audience scoping means any downstream API that decodes the token can read the user's roles in every other application and organisation. For a government procurement platform this creates unnecessary information disclosure.

### 4.4 Token Issuance

**FACT:** Tokens are signed RS256 with a 2048-bit RSA key loaded from the `PrivateKey` configuration key at startup. The `kid` is a hardcoded constant (`c2c3b22ac07f425eb893123de395464e`) in `AuthorityConfiguration.cs`. The `SecurityTokenDescriptor` in `CreateAccessToken` sets no `Audience` field.

**FACT:** Access token expiry is 3600 seconds and refresh token expiry is 86400 seconds, both hardcoded in `TokenService.CreateToken`. These values are not in `appsettings.json`.

**FACT:** The refresh token is composed as `{Base64(UTF8(URN))}:{rawToken}` where `rawToken` is a random value before PBKDF2 hashing. The salt is therefore the user's URN, making the URN recoverable from the token string by any party.

### 4.5 Standards Alignment

| Standard | Requirement | Status |
|---|---|---|
| OIDC Core 1.0 §3.1.3.7 | Validate `aud` contains client_id | FAIL — `ValidateAudience=false` |
| RFC 6749 §2.3 | Client authentication at token endpoint | FAIL — no client_id validated |
| RFC 7517 §4.5 | kid identifies the key | PARTIAL — kid present but static, not derived from key material |
| RFC 7638 | JWK Thumbprint for kid | FAIL — kid is a hardcoded string |
| OAuth 2.1 §4.1.1 | PKCE required for all auth code flows | FAIL — OrganisationApp has UsePkce=false |
| OWASP ASVS v4 §3.3.2 | Session invalidation on logout | PARTIAL — refresh revoked; access token not revocable |
| OWASP ASVS v4 §3.5.1 | JWT audience validation | FAIL — both validate steps disable audience |
| NCSC Cloud Security Principle 13 | Audit and accountability | FAIL — no structured auth events |

### 4.6 Gaps Summary

- Audience validation disabled at both token exchange (`TokenService.ValidateInternalAsync`) and API consumption (`AddJwtBearerAuthentication`).
- No `aud` claim emitted in issued Authority JWTs.
- `kid` is a static constant, not derived from key material; key rotation does not update `kid`.
- Access token revocation not implemented.
- PKCE suppressed in OrganisationApp.
- No structured auth audit log in the Authority service.
- No client validation at `POST /token`.
- Refresh token encodes user URN in the first segment.
- Token expiry values are hardcoded, not configurable.
- `SessionTimeoutInMinutes` missing from config produces silent zero TTL, not an explicit failure.

---

## 5. Application and Role Management Assessment

### 5.1 Application Import and Registration

**FACT:** No UI page exists for creating or registering an application. The `IAppRegistryClient` interface has no `CreateApplication` method and `AppRegistryClient.cs` has no POST to `/api/applications`. Applications must be seeded via the `POST /api/applications` REST endpoint (accessible only to PlatformAdmin) or directly into MongoDB.

**INFERENCE:** This is an operational gap. In a production procurement platform, new SaaS applications will need to be onboarded regularly. Without a UI, every new application onboarding requires direct API access or database manipulation — both of which bypass any approval workflow.

**RISK:** Applications seeded directly into MongoDB bypass all audit logging, role validation, and any future approval workflow. In a live environment this also requires elevated database credentials.

### 5.2 Organisation-Application Linkage

**FACT:** `POST /api/organisations/{orgId}/applications/{appId}` (enable an application for an organisation) returns `Results.Created()` with no repository call. `DELETE /api/organisations/{orgId}/applications/{appId}` returns `Results.NoContent()` with no repository call. Both are stub implementations in `OrganisationEndpoints.cs` lines 146–162.

**FACT:** There is no UI to perform this action; it is only callable via API, and the API stubs persist no data.

**RISK:** The organisation-application linkage is completely non-functional. An API consumer calling these endpoints receives a success response but no data is written. Any subsequent call to `GET /api/organisations/{orgId}/applications` will not reflect the operation.

### 5.3 User-to-Application Role Assignment

**FACT:** `POST /api/organisations/{orgId}/applications/{appId}/users` correctly calls `IUserAssignmentRepository.AddAssignmentAsync` and returns HTTP 201. The assignment is persisted to MongoDB with `AssignedBy = "system"` (hardcoded literal, `UserAssignmentEndpoints.cs` line 43).

**FACT:** `PUT .../users/{userId}` correctly updates roles. `DELETE .../users/{userId}` soft-deletes via `RevokeAssignmentAsync` setting `IsActive=false`. These operations are functionally correct in terms of data persistence.

**FACT:** A duplicate key violation (same UserPrincipalId + ApplicationId + OrganisationId) would result in an unhandled MongoDB exception propagating as HTTP 500. No conflict handling exists.

### 5.4 UI Capability

**FACT:** The `ApplicationList`, `ApplicationDetail`, `UserAssignments`, `AssignUser`, `RevokeConfirmation`, and `UserAssignmentCheckAnswers` pages all exist and are guarded by `OrgScopeRequirement.Admin` plus a runtime `org.IsBuyer()` check. These pages cover the read-list, view-detail, assign-user, edit-roles, and revoke-user flows.

**FACT:** After a successful assignment save or revoke, the user is silently redirected to the `UserAssignments` page with no GDS notification banner. No success feedback is presented.

**FACT:** The `UserAssignments` page displays an "effective permissions count" but not the actual permission names, limiting the ability of an OrgAdmin to audit what permissions a user currently has.

**FACT:** No UI exists for a PlatformAdmin (SupportAdmin or SuperAdmin) to manage applications globally, enable applications for organisations, or view all cross-organisation assignments.

### 5.5 Backend Enforcement

**FACT:** All six Application Registry pages in the Frontend enforce `OrgScopeRequirement.Admin` and `org.IsBuyer()` at runtime. The runtime check is a programmatic redirect to `/page-not-found`, not a declarative policy or HTTP 403.

**FACT:** API endpoints in the App Registry enforce authorization via the `IAuthorizationService` and the AppRegistry-specific authorization policies (`PlatformAdmin`, `OrgMember`, `OrgAdmin`) defined in `AppRegistryTestFactory.cs` and the wired endpoint map.

**INFERENCE:** The backend enforcement at the API level appears functionally correct in the areas exercised by the 44 endpoint tests. The gaps are in stub implementations (enable/disable) and audit trail, not in the authorization enforcement logic itself.

### 5.6 Gaps

- Organisation-application linkage (grant/revoke application to org) is entirely non-functional — stubs only.
- No application registration UI or workflow.
- No PlatformAdmin/SupportAdmin UI for global application management.
- `AssignedBy` is always `"system"` — no real caller identity captured.
- No success notification after assignment actions (violates GDS Design System pattern for confirmation feedback).
- Effective permissions displayed as a count, not as named permissions (limits auditability).
- SUPERADMIN scope constant defined but no pages enforce it.
- Duplicate assignment produces HTTP 500 instead of HTTP 409.
- No UI informs the OrgAdmin that role assignments do not take effect until the user's token is re-issued.

---

## 6. Token Claims Assessment

### 6.1 Actual Token Claim Shape (Code Evidence)

When `Features:ClaimsApiEnabled=true` and the Organisation API call succeeds:

```json
{
  "sub": "<UserUrn>",
  "channel": "one-login",
  "roles": "ADMIN,VIEWER",
  "cdp_claims": {
    "userPrincipalId": "<UserUrn>",
    "organisations": [
      {
        "organisationId": "<Guid>",
        "organisationName": "Example Org",
        "organisationRole": "ADMIN",
        "applications": [
          {
            "applicationId": "<Guid>",
            "applicationName": "Example App",
            "clientId": "client-abc",
            "roles": ["ReadOnly"],
            "permissions": ["report.view", "report.export"]
          }
        ]
      }
    ]
  }
}
```

When `Features:ClaimsApiEnabled=false` (the production default):

```json
{
  "sub": "<UserUrn>",
  "channel": "one-login",
  "roles": "ADMIN,VIEWER"
}
```

**Source:** `TokenService.CreateToken`, `TokenService.CreateAccessToken`, `GetUserClaimsUseCase` response shape. File: `/Services/CO.CDP.Organisation.Authority/TokenService.cs`.

### 6.2 Expected Claim Shape (Spec Requirement)

The spec requires application-scoped roles in the token. The intended shape (based on evidence of `GetClaimsTreeUseCase` and the `cdp_claims` shape already implemented) is functionally present in the code — the structure is correct. The gap is not structural but operational: the feature flag defaults to off.

A planned `cdp_app_claims` claim type was referenced in planning but does not exist anywhere in the codebase. The claim is `cdp_claims` only, defined as `Constants.ClaimType.CdpClaims`.

### 6.3 Application Roles in Token

**FACT:** Application roles are present in issued tokens IF AND ONLY IF `Features:ClaimsApiEnabled=true` is set in the environment configuration. The default in `appsettings.json` is `false`.

**FACT:** `GetClaimsTreeUseCase` exists and is wired to `GET /api/claims/{userPrincipalId}` but is NOT called by `TokenService`. Token enrichment uses `GetUserClaimsUseCase` (calling `GET /organisations/claims/users/{urn}`). These are two parallel read paths that return equivalent data.

**FACT:** There is no mechanism in the current code to force a token refresh when a user's role assignments change. An OrgAdmin who assigns a new role to a user will not see the effect until the user's access token expires (up to 1 hour) or they re-authenticate.

### 6.4 Claim Scoping

**FACT:** The `SecurityTokenDescriptor` in `CreateAccessToken` sets no `Audience` field. Audience validation is disabled at both the token exchange step and at API service validation.

**FACT:** `cdp_claims` embeds all roles for all organisations across all applications. There is no per-audience or per-service filtering. Any downstream API can decode the token and read the user's roles in organisations and applications it has no legitimate interest in.

**INFERENCE:** For the current internal-only deployment this represents an information disclosure risk that is limited in scope. If the JWT were ever intercepted or logged in full, the entire role profile of the user across all applications is exposed.

### 6.5 Security and Privacy Risks

| Risk | Severity | Evidence |
|---|---|---|
| Application roles absent from default token | CRITICAL | `appsettings.json`: `Features:ClaimsApiEnabled=false` |
| All org/app roles in single token (no audience scoping) | MEDIUM | No `aud` claim; no filtering in `GetUserClaimsUseCase` |
| Role changes not reflected until token re-issue (up to 1h) | MEDIUM | Hardcoded 3600s expiry; no revocation list for access tokens |
| URN recoverable from refresh token string | MEDIUM | `TokenService.CreateRefreshToken`: salt = Base64(UTF8(URN)) |
| `cdp_app_claims` constant referenced in planning does not exist | LOW | Confirmed by codebase search: no such claim type |
| `org` claim never written but read by `ClaimService.GetOrganisationId()` | LOW | `TokenService.CreateAccessToken` has no org claim; `ClaimService.GetOrganisationId()` always returns null |

---

## 7. Critical Gaps and Unspoken Truth

This section states the direct, unvarnished assessment of each critical gap.

### Gap 1: The application-role feature is off by default in production

The entire feature/app-roles branch exists to put application-scoped roles in the JWT. The mechanism that writes those roles (`Features:ClaimsApiEnabled`) is `false` in `appsettings.json`. Unless the deployment pipeline explicitly sets this flag, the branch delivers zero new authorisation capability compared to the previous codebase. There is no safety net: no startup warning, no assertion, no health check. The service starts silently and issues structureless tokens.

**File:** `/Services/CO.CDP.Organisation.Authority/appsettings.json`

### Gap 2: Audience validation is disabled everywhere that matters

`ValidateAudience=false` appears in two separate files. Neither is justified by a comment. OIDC Core 1.0 §3.1.3.7 is unambiguous: the `aud` claim MUST be validated. The current code accepts any validly signed JWT from the correct issuer at both the token exchange endpoint and at all downstream API services. This is not a design trade-off; it is a missing implementation.

**Files:** `TokenService.cs (ValidateInternalAsync)`, `AuthenticationExtensions.cs (AddJwtBearerAuthentication)`

### Gap 3: The organisation-application linkage API does not work

The enable and disable endpoints exist in the route map and return success HTTP codes. They have never persisted a single byte to MongoDB. Any operational runbook, integration test, or partner service that calls these endpoints is being silently misled. This is not a TODO comment — it is a stub that returns success.

**File:** `OrganisationEndpoints.cs` lines 146–162

### Gap 4: The audit trail is a fiction

Every `AuditLog` record in every MongoDB repository says it was performed by `"system"`. Every `UserApplicationAssignment` record says it was created by `"system"`. In an NCSC-assessed government procurement system, "who did what and when" is a regulatory requirement, not a nice-to-have. The AuditLog table exists; it is simply never populated with real data.

**Files:** `MongoApplicationRepository.cs`, `MongoUserAssignmentRepository.cs`, `MongoOrganisationRepository.cs`, `UserAssignmentEndpoints.cs`

### Gap 5: Refresh tokens encode the user's identity

The refresh token format is `{Base64(URN)}:{randomValue}` with PBKDF2 applied to `{randomValue}` using `UTF8(URN)` as the salt. Any party who receives a refresh token (e.g. from a log, a network capture, or a storage leak) can recover the user's URN by Base64-decoding the first segment. The token was designed to be opaque. It is not.

**File:** `TokenService.cs (CreateRefreshToken)`

### Gap 6: The end-to-end security flow has never been tested as a whole

The test suite has 98 tests. None of them exercise the complete path: a real (or realistic) OneLogin JWT → `ValidateOneLoginToken` → `CreateToken` (calling Organisation API for cdp_claims) → issued Authority JWT → `ClaimService.HasApplicationRole` returning the correct boolean. The unit tests are individually sound but the joints between them are untested. If the JSON shape of `cdp_claims` drifts between `GetUserClaimsUseCase` and `ClaimService.GetApplicationClaims`, no test will catch it until a user finds they cannot access a resource they should be able to access.

**Files:** `TokenServiceTest.cs`, `ClaimServiceTests.cs`, `ApplicationRegistryFunctionalTests.cs`

### Gap 7: Role assignments silently take up to 1 hour to appear in tokens

The UI allows an OrgAdmin to assign roles to a user. After saving, the user is silently redirected back to the list. There is no indication anywhere in the UI or documentation that the assignment will not take effect until the user's current token expires (up to 1 hour) or they re-authenticate. This is an operational support gap: an OrgAdmin will assign a role, the user will complain they cannot access a resource, and neither party has the information to diagnose the cause.

**Files:** `UserAssignmentCheckAnswers.cshtml.cs`, `AssignUser.cshtml.cs`

### Gap 8: SUPERADMIN is defined but enforced nowhere

The `PersonScopes.SUPERADMIN` constant and the `PersonScopeRequirement.SuperAdmin` policy exist. No page in the Frontend is guarded by this requirement. The highest privilege level in the system is a dead code path. SuperAdmins can log in, receive a token with `roles=SUPERADMIN`, and find they have access to nothing that ordinary users do not already have access to via the SupportAdmin path.

**Files:** `AuthorizationExtensions.cs`, all files in `CO.CDP.OrganisationApp/Pages/`

---

## 8. Test Plan

### 8.1 Unit Tests — Authority TokenService

| Test Case | Method | Input | Expected | Missing? |
|---|---|---|---|---|
| Expired refresh token | `ValidateAndRevokeRefreshToken` | `RefreshToken.ExpiryDate = DateTime.UtcNow.AddMinutes(-1)` | `(false, null)` | YES |
| Expired OneLogin JWT | `ValidateOneLoginToken` | JWT with `exp = DateTime.UtcNow.AddMinutes(-1)` | `(false, null)` | YES |
| Wrong issuer in OneLogin JWT | `ValidateOneLoginToken` | JWT with `iss = "https://wrong.issuer"` | `(false, null)` | YES |
| Wrong signing algorithm | `ValidateOneLoginToken` | JWT signed with HS256 | `(false, null)` | YES |
| Missing `sub` claim | `ValidateOneLoginToken` | JWT with no `sub` claim | `(false, null)` | YES |
| Organisation API returns 500 | `CreateToken` (ClaimsApiEnabled=true) | HTTP 500 from Organisation API | Token issued without cdp_claims | YES |
| Organisation API returns 503 | `CreateToken` (ClaimsApiEnabled=true) | HTTP 503 from Organisation API | Token issued without cdp_claims | YES |
| Token expiry fields | `CreateToken` | ClaimsApiEnabled=true, valid response | `ExpiresIn=3600`, `RefreshTokenExpiresIn=86400` | YES |
| Missing `grant_type` | `POST /token` endpoint | Form body with no grant_type | HTTP 400 | YES |
| Missing `client_secret` | `POST /token` (client_credentials) | Form body with no client_secret | HTTP 400 | YES |
| Missing `refresh_token` | `POST /token` (refresh_token) | Form body with no refresh_token | HTTP 400 | YES |

### 8.2 Unit Tests — ClaimService

| Test Case | Method | Input | Expected | Missing? |
|---|---|---|---|---|
| Wrong orgId for HasApplicationPermission | `HasApplicationPermission` | cdp_claims with different orgId | `false` | YES |
| Wrong clientId for HasApplicationPermission | `HasApplicationPermission` | cdp_claims with different clientId | `false` | YES |
| Multi-org claims tree — role in second org | `HasApplicationRole` | cdp_claims with 2 orgs, role only in second | `true` (matching second org) | YES |
| Multi-org claims tree — permission deduplication | `HasApplicationPermission` | Permission in two roles in two orgs | `true` (matching org) | YES |
| `GetApplicationClaims` — schema mismatch | `GetApplicationClaims` | Valid JSON but missing required fields | Graceful null or partial | YES |

### 8.3 Unit Tests — GetClaimsTreeUseCase

| Test Case | Description | Missing? |
|---|---|---|
| Disabled application excluded | User has assignment on app with `IsActive=false`; should not appear in tree | YES |
| Multiple orgs, partial membership | User is member of org A and B; has app assignments only in B | YES |
| Permission deduplication across 3+ roles | 3 roles all sharing 1 permission; permission appears once in output | Partial — currently tested for 2 roles |

### 8.4 Integration Tests

The following integration tests do not currently exist and are necessary for a government service of this security classification.

| Test | Description | Priority |
|---|---|---|
| Login → token → role check | Wire a test OneLogin JWT (signed with a test RSA key) through `ValidateOneLoginToken` → `CreateToken` (with mocked but realistic Organisation API response) → `ClaimService.HasApplicationRole` asserts correct boolean | CRITICAL |
| Token expiry propagation | Issue token, wait for expiry (with fast-forwarded clock), assert token is rejected at API service | HIGH |
| Refresh token rotation | Issue token, refresh, assert original refresh token revoked | HIGH |
| cdp_claims JSON contract | Assert that the JSON string embedded in the JWT by `TokenService` can be deserialised by `ClaimService.GetApplicationClaims` without loss | HIGH |
| Feature flag gate | Assert that with `ClaimsApiEnabled=false`, Organisation API is never called and `cdp_claims` is absent | MEDIUM |

### 8.5 E2E Tests

| Test | Description | Status |
|---|---|---|
| Unauthenticated user redirected from Application Registry pages | GET /organisation/{id}/applications with no auth cookie → redirect to login | MISSING |
| Non-Buyer org user cannot see Application Registry links | Supplier org user → no 'Manage application access' link visible | MISSING |
| Non-Admin org member forbidden | OrgViewer attempts to access /user-assignments → 403 | MISSING |
| Assign role → token refreshed → role enforced | Full E2E: assign role, log user out and back in, assert new role present | MISSING |
| Success banner after assignment save | Assign user, confirm → GDS success notification displayed | MISSING |

### 8.6 Security / Negative Tests

| Test | Target | Expected |
|---|---|---|
| Replay OneLogin token at POST /token after revocation | `POST /token` | HTTP 400 (or 401) if token reuse is detectable |
| Present Authority token with wrong audience at API service | `AddJwtBearerAuthentication` | HTTP 401 (currently would return 200 — audience not validated) |
| Present Authority token issued to one service at another | API service B validates token intended for API service A | HTTP 401 (currently would return 200) |
| Recover URN from refresh token without database | `CreateRefreshToken` output | URN recoverable — confirms the gap |
| Duplicate user assignment | `POST /api/organisations/{orgId}/applications/{appId}/users` twice | HTTP 409 (currently HTTP 500) |

---

## 9. Implementation Backlog

| Priority | # | Title | Description | Acceptance Criteria | Files to Change | Test Requirement |
|---|---|---|---|---|---|---|
| CRITICAL | 1 | Enable `cdp_claims` by default | Set `Features:ClaimsApiEnabled=true` in `appsettings.json` and all environment config files. Add a startup assertion that logs a WARNING (not an error) if the flag is false. | Tokens issued in a default deployment contain `cdp_claims`; startup logs a warning when disabled | `appsettings.json`, `appsettings.Development.json`, deployment env config | Regression: `CreateToken` with flag true produces `cdp_claims`; flag false omits it and logs warning |
| CRITICAL | 2 | Enable audience validation on incoming OneLogin tokens | In `TokenService.ValidateInternalAsync`, set `ValidateAudience=true` and `ValidAudiences=new[]{clientId}` where `clientId` is loaded from `IAuthorityConfiguration`. | A OneLogin JWT with `aud` not containing the configured `clientId` is rejected with `(false, null)`. | `TokenService.cs (ValidateInternalAsync)` | Unit test: JWT with wrong aud → `(false, null)`; correct aud → `(true, urn)` |
| CRITICAL | 3 | Emit and validate `aud` claim in Authority JWTs | Set `Audience = configuration.AuthorityUrl` (or a dedicated resource identifier) in `CreateAccessToken`'s `SecurityTokenDescriptor`. Enable `ValidateAudience=true` in `AddJwtBearerAuthentication`. | Authority JWTs carry an `aud` claim; API services reject tokens with mismatched audience. | `TokenService.cs (CreateAccessToken)`, `AuthenticationExtensions.cs (AddJwtBearerAuthentication)` | Integration test: token without aud rejected at API; token with correct aud accepted |
| CRITICAL | 4 | Implement organisation-application linkage stubs | Replace stub bodies in `POST /api/organisations/{orgId}/applications/{appId}` and `DELETE` counterpart with real `IOrganisationApplicationRepository.LinkApplicationAsync` / `UnlinkApplicationAsync` calls. | POST returns 201 and MongoDB contains the linkage; DELETE returns 204 and linkage is removed or marked inactive; GET list reflects the change. | `OrganisationEndpoints.cs` (lines 146–162), `IOrganisationApplicationRepository.cs`, `MongoOrganisationApplicationRepository.cs` | Unit test: stub replaced; integration test: POST → GET reflects linkage; DELETE → GET omits linkage |
| HIGH | 5 | Replace hardcoded `"system"` audit identity with real caller | Extract caller URN from `IHttpContextAccessor` (via `ClaimService.GetUserUrn`) in `UserAssignmentEndpoints.cs` and pass to `AddAssignmentAsync`. Update all three MongoDB repositories to accept and store real `UserId`. | `AuditLog.UserId` and `UserApplicationAssignment.AssignedBy` contain the authenticated caller's URN for every write operation; `"system"` literal is removed from all write paths. | `UserAssignmentEndpoints.cs`, `MongoApplicationRepository.cs`, `MongoUserAssignmentRepository.cs`, `MongoOrganisationRepository.cs` | Unit test: assignment endpoint called by user X → `AssignedBy=X` in persisted record |
| HIGH | 6 | Fix refresh token salt — remove URN encoding | Replace `salt = Encoding.UTF8.GetBytes(urn)` with a cryptographically random 32-byte salt. Store the URN separately in the `RefreshToken` database record. Update `ValidateAndRevokeRefreshToken` to retrieve URN from the record, not from the token string. | Refresh token string contains no decodable user identifier; URN is retrieved from the database record on validation. | `TokenService.cs (CreateRefreshToken, ValidateAndRevokeRefreshToken)` | Unit test: Base64-decode of first token segment does not yield a valid URN |
| HIGH | 7 | Derive `kid` from key material using RFC 7638 JWK Thumbprint | Compute the SHA-256 thumbprint of the RSA public key on load in `ConfigurationService`. Use this as `kid` in token headers and JWKS response. Remove the hardcoded constant from `AuthorityConfiguration.cs`. | `kid` in issued tokens matches the SHA-256 thumbprint of the current public key; `kid` changes when the private key PEM is replaced; JWKS `kid` and token `kid` always match. | `ConfigurationService.cs`, `AuthorityConfiguration.cs`, `GenerateKeys.cs` | Unit test: key loaded → kid computed; key replaced → kid updated |
| HIGH | 8 | Implement structured auth audit logging in the Authority service | Add structured `ILogger` events (using `LoggerMessage.Define`) in `TokenService` for: token issuance (success), token issuance (failure), refresh token validation (success/failure), refresh token revocation. Include fields: `{Event, URN, GrantType, Success, ErrorCode, Timestamp}`. | Log aggregation (CloudWatch/equivalent) shows structured auth events with all required fields; no auth event goes unlogged. | `TokenService.cs` | Unit test: mock `ILogger`; assert `LogInformation` called with correct structured properties on success/failure paths |
| MEDIUM | 9 | Enable PKCE for OrganisationApp | Set `UsePkce = true` in OrganisationApp's OIDC options. Verify compatibility with `private_key_jwt` client authentication (both can coexist). | OrganisationApp authorization code flow includes `code_challenge` and `code_verifier`; OneLogin validates them correctly; existing authentication flow is unbroken. | OrganisationApp `Program.cs` or OIDC configuration class | E2E: login flow completes successfully with PKCE enabled |
| MEDIUM | 10 | Handle duplicate user assignment as HTTP 409 | Catch `MongoDuplicateKeyException` (or `MongoWriteException` with code 11000) in `UserAssignmentEndpoints.cs` POST handler. Return HTTP 409 with a descriptive RFC 7807 problem body. | Duplicate assignment attempt returns HTTP 409 with `detail` field; no HTTP 500 surfaces to caller. | `UserAssignmentEndpoints.cs`, add exception-handling middleware or inline try/catch | Unit test: repository mock throws `MongoWriteException(11000)` → endpoint returns 409 |
| MEDIUM | 11 | Add `IsActive` filter to AccessControlEntry queries | Add `IsActive` boolean field to `AccessControlEntry` entity. Update `GetAclEntriesAsync` in `IAccessControlRepository` implementation to filter `a.IsActive == true`. Add soft-delete on revocation. | ACEs with `RevokedAt != null` or `IsActive=false` are excluded from all authorisation checks; revocation takes effect immediately. | `AccessControlEntry.cs`, `IAccessControlRepository.cs` implementation, migration or index creation | Unit test: revoked ACE not returned by `GetAclEntriesAsync` |
| MEDIUM | 12 | Add GDS success notification banner after assignment save | Display a GDS `govuk-notification-banner--success` after `AssignUserAsync` or `UpdateUserRolesAsync` completes. Include message: "Role assignment saved. Changes will take effect when the user next signs in." | OrgAdmin sees a success banner on the UserAssignments page after saving or revoking a role assignment; banner text informs the admin of the token refresh delay. | `UserAssignments.cshtml`, `UserAssignmentCheckAnswers.cshtml.cs`, `RevokeConfirmation.cshtml.cs` | E2E: assign role → confirm → banner displayed on redirect |
| LOW | 13 | Move token expiry values to configuration | Extract `3600` and `86400` from `TokenService.CreateToken` into `IAuthorityConfiguration` with defaults of 3600 and 86400. Load from `appsettings.json` keys `TokenExpirySeconds` and `RefreshTokenExpirySeconds`. | Token expiry values are configurable per environment without code change; defaults match current behaviour. | `TokenService.cs`, `AuthorityConfiguration.cs`, `appsettings.json` | Unit test: configuration values propagate to issued token `exp` claim |
| LOW | 14 | Guard against missing `SessionTimeoutInMinutes` | In `LogoutManager`, replace `GetValue<double>("SessionTimeoutInMinutes")` with a required value load that throws `InvalidOperationException` with a descriptive message if missing. Add the key to all `appsettings.*.json` files. | Application fails to start (or logs ERROR at startup validation) if `SessionTimeoutInMinutes` is absent from configuration; no silent zero TTL. | `LogoutManager.cs`, all `appsettings.*.json` files | Unit test: missing config key throws `InvalidOperationException` |

---

## 10. Final Recommendation

**Classification: ARCHITECTURALLY INCOMPLETE (Demo-Only in Current State)**

The Application Registry data model is well-structured. The MongoDB repositories are correctly implemented for the operations they cover. The UI pages are correctly guarded. The claims assembly code is present and structurally correct. These are genuine achievements on the feature/app-roles branch.

However, the system cannot be shipped to production as a security-classified government procurement service for the following reasons, which are not implementation details but architectural gaps:

1. The core deliverable — application roles in JWT — is disabled by default and silently absent from all tokens in any environment where the feature flag is not explicitly overridden.
2. Audience validation is disabled at the two most critical validation points in the system, violating a mandatory OIDC Core 1.0 requirement.
3. The enable/disable organisation-application linkage API, a prerequisite for any operational role-assignment workflow, is a stub that persists no data.
4. The audit trail, a regulatory requirement for a government service, records every write as performed by `"system"`.
5. The end-to-end authentication and authorisation flow has never been tested as an integrated path.

This branch is appropriate as a functional demonstration of the Application Registry UI, the claims data model, and the MongoDB persistence layer. It is not appropriate for a production deployment to an environment serving real procurement transactions or real government users until all five of the blockers above are resolved, and the remaining HIGH-priority backlog items are addressed.

**Recommended Next Action:** The top 4 items in Section 9 (enable flag, audience validation, aud claim, and stub implementations) should be completed as a single hardening sprint before any further feature development. Items 5 and 6 (audit trail and refresh token salt) should follow immediately. Only after those 6 items are complete should the branch be considered for a security assessment.

---

*Report generated from static analysis of the feature/app-roles branch. All findings cite specific files and methods. Where source code was not examined, the finding is marked "No repository evidence found". This report does not constitute a formal penetration test or NCSC assessment.*
