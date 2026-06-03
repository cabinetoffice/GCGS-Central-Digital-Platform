# CO-CDP Identity & Application Registry — Full Feature Audit Report

**Branch:** feature/app-roles
**Date:** 2026-06-03
**Reviewer classification:** Senior Identity Architect / OIDC Security Reviewer / Full-Stack Feature Auditor / Functional Test Engineer
**Scope:** OIDC/OAuth flow, Application Registry data model, claim assignment and token issuance, UI capability, infrastructure, and test coverage.

---

## 1. Executive Summary

### Overall Readiness: AMBER

The feature/app-roles branch delivers a structurally sound Application Registry with a working role-per-application data model, a fully implemented token exchange service, and a GOV.UK-compliant frontend for Org Admin user assignment. The most recent hardening sprint addressed a significant number of real security defects: PKCE is now enforced, audience validation is conditionally live, `aud` is emitted in issued JWTs, the `kid` is now RFC 7638-derived, refresh tokens carry the user URN and salt, and caller identity has been replaced from the hardcoded "system" string with the real `sub` claim.

However, several blockers remain that prevent unqualified production deployment. The most significant is the token enrichment gap: org-scoped role claims (`org:{orgId}:role`) must be pre-baked into the bearer token at login time for the `OrgAdmin` and `OrgMember` policies to function, but there is no mechanism in the current codebase that injects these claims into the Authority-issued JWT. This means the `OrgAdmin` and `OrgMember` authorization policies will fail for any real user who is not a platform admin. Additionally, there is no Platform Admin UI, meaning application registration and role/permission management can only be performed via direct API or database seeding. The Authority is not a full OIDC Authorization Server and should not be treated as one.

### Top 5 Blockers

1. **Token enrichment gap (CRITICAL):** The `OrganisationRoleHandler` reads `org:{orgId}:role` claims from the JWT. No code path in the Authority's `CreateAccessToken` writes these claims into the token. `OrgAdmin` and `OrgMember` policies will deny all non-platform-admin users.
2. **No Platform Admin UI (HIGH):** Application registration, role/permission management, and org-application linking have no frontend. These operations require direct API calls or MongoDB seeding.
3. **Audience validation disabled in development (HIGH):** When `OneLogin:ClientId` is absent from config (confirmed in `appsettings.Development.json`), `ValidateAudience=false` is set with only a warning log. Any OneLogin-signed token is accepted in local development regardless of intended audience.
4. **Org-application disable is a hard delete (MEDIUM):** `DisableApplicationAsync` uses `PullFilter` to physically remove the `OrganisationApplication` embedded document. There is no `IsEnabled` flag, no `DisabledAt`, no `DisabledBy`. Historical state is unrecoverable without trawling audit logs.
5. **Dangling `RolePermission` references (MEDIUM):** `DeletePermissionAsync` hard-removes the `ApplicationPermission` document. `RolePermission` records that reference the deleted permission ID become dangling references. No cascade logic exists at the data layer.

### Uncomfortable Truth

The feature works end-to-end only for Platform Admins. For any regular org member or org admin, the authorization policies `OrgAdmin` and `OrgMember` will silently deny access because the required `org:{orgId}:role` claims are never written into the Authority JWT. The success notification banners ("Changes take effect when the user next signs in") are accurate in spirit but misleading in practice — role changes in the Application Registry have no effect on token content because `GetClaimsTreeUseCase` is not wired into any token-issuance code path.

---

## 2. Repository Feature Map

### Relevant Files and Modules

| Module | File(s) | Role |
|---|---|---|
| Authority token exchange | `Services/CO.CDP.Organisation.Authority/TokenService.cs` | Validates OneLogin tokens; issues Authority JWTs |
| Authority endpoints | `Services/CO.CDP.Organisation.Authority/Endpoint.cs` | `POST /token`, `POST /revocation`, OIDC discovery, JWKS |
| Authority configuration | `Services/CO.CDP.Organisation.Authority/ConfigurationService.cs` | Derives `kid` via RFC 7638; loads RSA key |
| Authority data model | `Services/CO.CDP.Organisation.Authority/Model/AuthorityConfiguration.cs` | Config defaults; hardcoded `kid` fallback |
| OIDC middleware (frontend) | `Frontend/CO.CDP.OrganisationApp/Program.cs` | Registers OIDC with OneLogin; `UsePkce=true` |
| OIDC events | `Libraries/CO.CDP.Authentication/Services/OidcEventsService.cs` | Token exchange hook; private key JWT client assertion |
| Token exchange (client) | `Libraries/CO.CDP.Authentication/Services/TokenExchangeService.cs` | POSTs `grant_type=client_credentials` to Authority `/token` |
| Logout | `Libraries/CO.CDP.Authentication/Services/LogoutManager.cs` | Forwards logout notifications; `sirsi-logout-api-key` header |
| Authentication extensions | `Libraries/CO.CDP.Authentication/Extensions.cs` | Registers auth middleware; `AddJwtBearerAuthentication` |
| Claim service | `Libraries/CO.CDP.Authentication/ClaimService.cs` | `HasApplicationRole`, `HasApplicationPermission`, `GetApplicationClaims` |
| ApplicationRegistry persistence | `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Persistence/` | MongoDB repositories for Application, Organisation, UserAssignment |
| ApplicationRegistry endpoints | `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Endpoints/` | REST API for all CRUD operations |
| Authorization policies | `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/AuthorizationPolicies.cs` | PlatformAdmin, OrgAdmin, OrgMember policies |
| Claims use cases | `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/UseCase/Claims/` | `GetClaimsTreeUseCase`, `GetUserClaimsUseCase` |
| Frontend UI pages | `Frontend/CO.CDP.OrganisationApp/Pages/Organisation/Applications/` | ApplicationList, ApplicationDetail, UserAssignments, AssignUser, RevokeConfirmation |
| Frontend API client | `Frontend/CO.CDP.OrganisationApp/WebApiClients/IAppRegistryClient.cs` | HTTP client interface for Application Registry API |
| Refresh token entity | `Services/CO.CDP.OrganisationInformation.Persistence/RefreshToken.cs` | PBKDF2-hashed, database-backed; `UserUrn`, `Salt` added |
| Organisation API claims endpoint | `Services/CO.CDP.Organisation.WebApi/Organisation.cs` line 66 | `GET /organisations/claims/users/{urn}` — called by `TokenService` |

### Identity / OIDC Flow (step by step)

1. User visits `OrganisationApp`. Unauthenticated request triggers OIDC redirect.
2. `Program.cs` — ASP.NET Core OIDC middleware redirects user to GOV.UK One Login `/authorize` with PKCE (`UsePkce=true`, `Program.cs` line 299).
3. One Login authenticates the user (MFA enforced via VTR). Callback returns authorization code.
4. `OidcEventsService.OnTokenValidated` fires — a private key JWT client assertion is constructed (`SecurityAlgorithms.RsaSha256`) and the OneLogin access token is obtained.
5. `TokenExchangeService.ExchangeAsync` POSTs `grant_type=client_credentials`, `client_secret=<onelogin_access_token>` to Authority `POST /token`.
6. Authority `TokenService.ValidateOneLoginToken` validates the OneLogin token using JWKS from OneLogin's discovery document. Audience validation is conditional on `OneLogin:ClientId` being configured.
7. `TokenService.CreateAccessToken` issues an Authority JWT: `sub`, `channel=one-login`, `roles` (platform scopes), `cdp_claims` (JSON blob, if `ClaimsApiEnabled=true`), signed with RS256 using the configured RSA private key.
8. Authority also issues an opaque refresh token (PBKDF2-hashed, stored in PostgreSQL `RefreshToken` table).
9. Subsequent API calls to Organisation WebApi include the Authority JWT as Bearer token. `AddJwtBearerAuthentication` validates signature, `aud`, `iss`, and `exp`.

### Application Import / Registration Flow

FACT: There is no import flow. Applications are registered via `POST /api/applications` (Platform Admin only). No UI exists. Seeding is done directly via API or database tooling.

### User / Application / Role Assignment Flow

1. OrgAdmin navigates to `/organisation/{id}/applications` (`ApplicationList` page).
2. Selects an application → `/organisation/{id}/applications/{appId}` (`ApplicationDetail`).
3. Clicks "Manage user assignments" → `/organisation/{id}/applications/{appId}/user-assignments` (`UserAssignments`).
4. Clicks "Assign user" → `AssignUser` page. `AssignUserModel.OnGet` calls `GetApplicationRolesAsync`, `GetOrganisationMembersAsync`, `GetUserAssignmentsAsync`; filters member dropdown to exclude already-assigned users.
5. Form POST → `UserAssignmentState` serialised to TempData → redirect to `UserAssignmentCheckAnswers`.
6. Check Answers POST → `appRegistryClient.AssignUserAsync(orgId, appId, CreateAppRegistryUserAssignment)` → `POST /api/organisations/{orgId}/applications/{appId}/users` → `MongoUserAssignmentRepository.CreateAssignmentAsync`.
7. Redirect to `UserAssignments` with `TempData["SuccessMessage"]` = "User assigned. Changes take effect when the user next signs in."

### Token Issuing Flow (detail)

`TokenService.CreateAccessToken` (`TokenService.cs` lines 173–219):
- Fetches `OrganisationPerson` record for the URN to derive platform `roles`.
- If `ClaimsApiEnabled=true`, calls `GET /organisations/claims/users/{encodedUrn}` via named `HttpClient`; embeds raw JSON as `cdp_claims` claim.
- Sets `SecurityTokenDescriptor` with `sub`, `channel`, `roles`, `cdp_claims`, `Issuer`, `Audience=Issuer`, `Expires`, `SigningCredentials` (RS256).
- `kid` set explicitly on token header from `config.Kid` (RFC 7638 thumbprint in production).

### Client Consumption Flow

Client API receives Bearer JWT → `AddJwtBearerAuthentication` validates signature via JWKS from `/.well-known/openid-configuration/jwks` → `ClaimService.HasApplicationRole(orgId, clientId, roleName)` reads `cdp_claims` claim → filters by `organisationId` + `clientId` → checks role name. No database call at validation time.

---

## 3. Evidence Table

| # | Capability | Expected | Evidence | Status | Risk | Recommendation |
|---|---|---|---|---|---|---|
| 1 | PKCE enforced on OrganisationApp OIDC | Required (OAuth 2.1) | `Program.cs` line 299: `UsePkce=true` | PASS | LOW | No action |
| 2 | RS256 token signing | RS256 | `TokenService.cs` line 214: `SecurityAlgorithms.RsaSha256` | PASS | LOW | No action |
| 3 | `kid` derived from key material | RFC 7638 thumbprint | `ConfigurationService.cs` line 34: `ComputeJwkThumbprint`; hardcoded fallback `AuthorityConfiguration.cs` line 32 | PASS (with caveat) | LOW | Remove hardcoded fallback or document it clearly |
| 4 | `aud` claim emitted in issued JWT | Yes | `TokenService.cs` line 213: `Audience = config.Issuer` | PASS | LOW | Consider whether `aud` should equal the resource server URL, not the issuer |
| 5 | `aud` validated on incoming OneLogin tokens | Yes | `TokenService.cs` lines 103–107: conditional on `OneLoginClientId`; `ValidateAudience=false` when absent | PARTIAL | HIGH | Enforce `OneLoginClientId` as a required config value; add startup assertion |
| 6 | `aud` validated on Authority JWTs by consumers | Yes | `Libraries/CO.CDP.Authentication/Extensions.cs`: `AddJwtBearerAuthentication`; `ValidateAudience=true` now set | PASS | LOW | No action |
| 7 | OIDC discovery document | Required | `Endpoint.cs` lines 16–36: `GET /.well-known/openid-configuration` | PASS | LOW | No action |
| 8 | JWKS endpoint | Required | `Endpoint.cs` lines 38–55: `GET /.well-known/openid-configuration/jwks` | PASS | LOW | Add `Cache-Control` headers |
| 9 | `/token` endpoint | Required | `Endpoint.cs` line 57: `POST /token`; `client_credentials` + `refresh_token` | PASS | LOW | No action |
| 10 | `/revocation` endpoint | RFC 7009 | `Endpoint.cs` lines 88–105: `POST /revocation`; refresh tokens only | PARTIAL | MEDIUM | Return 200 (not 400) for unsupported `token_type_hint`; add logging on revocation failure |
| 11 | Refresh token is opaque and hashed | Security requirement | `TokenService.cs` lines 225–246: PBKDF2-HMAC-SHA256, 100k iterations, random salt | PASS | LOW | No action |
| 12 | Refresh token is single-use | Security requirement | `TokenService.cs` line 133: `Revoked=true` on redemption | PASS | MEDIUM | Add explicit DB transaction around revoke-and-reissue |
| 13 | `UserUrn` stored in refresh token record | Required for opacity | `RefreshToken.cs`: `UserUrn` and `Salt` fields present | PASS | LOW | No action |
| 14 | Application roles are application-scoped | Design requirement | `ApplicationRole.ApplicationId` FK; `GetRolesAsync` filters by application; `UserApplicationAssignment` unique index on `(user, app, org)` | PASS | LOW | No action |
| 15 | User can have different roles in App A vs App B | Design requirement | Separate `UserApplicationAssignment` documents per `(user, app, org)` triple | PASS | LOW | No action |
| 16 | Org-scoped role claims in Authority JWT | Required for auth policies | No code path in `CreateAccessToken` writes `org:{orgId}:role` claims; `OrganisationRoleHandler` reads these from token only | FAIL | CRITICAL | Enrich Authority JWT with org-scoped role claims from `GetClaimsTreeUseCase` or `OrganisationPerson` DB |
| 17 | OrgAdmin and OrgMember policies function for real users | Required for authorization | `OrganisationRoleHandler` inspects token claims only; claims not present → policies deny | FAIL | CRITICAL | See item 16 |
| 18 | `cdp_claims` embedded in token when enabled | Required for `HasApplicationRole` | `TokenService.cs` lines 181–205: calls `GET /organisations/claims/users/{urn}` when `ClaimsApiEnabled=true` | PASS | MEDIUM | Failure is silent (graceful degradation); surface error to caller |
| 19 | Role leakage across applications | Must not occur | `ClaimService.HasApplicationRole` requires exact `organisationId` + `clientId` match | PASS | LOW | No action |
| 20 | Platform Admin UI for application registration | Needed for operations | No frontend page calls `POST /api/applications` | FAIL | HIGH | Build PlatformAdmin portal or admin Swagger/tooling workflow |
| 21 | Org-application disable is soft/reversible | Auditability | `PullFilter` physically removes the embedded document; no `IsEnabled` flag | FAIL | MEDIUM | Add `IsEnabled`/`DisabledAt`/`DisabledBy` to `OrganisationApplication` |
| 22 | Permission deletion is safe | Data integrity | `DeletePermissionAsync` hard-removes; `RolePermission.PermissionId` becomes dangling | FAIL | MEDIUM | Add cascade delete or `IsActive` flag on `ApplicationPermission` |
| 23 | Caller identity stored on audit entries | Accountability | `callerUrn = context.User?.FindFirst("sub")?.Value ?? "system"` (`UserAssignmentEndpoints.cs` line 40–41) | PASS | LOW | No action |
| 24 | Single-item GetById skips `IsActive` filter | Security | `GetByIdAsync` for Application, Role, Organisation does not filter `IsActive` | FAIL | MEDIUM | Add `IsActive` guard to all `GetByIdAsync` methods |
| 25 | Consistent identity type across entities | Data quality | `EnabledBy`: string; `GrantedBy`: Guid; `UserId` in AuditLog: string; `UpdatedBy` in FeatureFlag: Guid | FAIL | LOW | Standardise to string (URN) across all entities |
| 26 | `ClaimsApiEnabled` production default | Should be true | `appsettings.json` line 29: `"ClaimsApiEnabled": true` | PASS | LOW | No action |
| 27 | UserRoleAssignment cross-application guard | Data integrity | No validation that `RoleId` belongs to the same `ApplicationId` as the `UserApplicationAssignment` | FAIL | MEDIUM | Add validation in `AssignUserAsync` use case |
| 28 | Refresh token expiry tested | Test coverage | No test in `TokenServiceTest.cs` for expired `RefreshToken` record | GAP | MEDIUM | Add test: valid format, DB record with `ExpiryDate < now` |
| 29 | Audience validation disabled — no startup guard | Security | `appsettings.Development.json` has no `OneLogin:ClientId`; validation silently skipped | FAIL | HIGH | Add `IStartupFilter` or `ValidateOnStart` assertion |
| 30 | JWKS endpoint caching headers | Performance | `Endpoint.cs` lines 38–55: no `Cache-Control` header on JWKS response | FAIL | LOW | Add `Cache-Control: public, max-age=3600` |

---

## 4. OIDC Compliance Assessment

### Authorisation Endpoint

FACT: No `/connect/authorize` endpoint exists anywhere in the Authority service (`Endpoint.cs` — no `MapGet` or `MapPost` for `/connect/authorize`). The Authority is a token exchange service, not a full OIDC Authorization Server. It has no authorization code flow, no implicit flow, and no ID token. The discovery document at `Endpoint.cs` line 30 advertises `ResponseTypesSupported = ["token"]` (implicit flow) but this is misleading — no such flow is implemented. The discovery document should omit `ResponseTypesSupported` or set it accurately to `[]`.

### Token Endpoint

`POST /token` (`Endpoint.cs` line 57). Two grant types: `client_credentials` (non-standard — OneLogin access token passed as `client_secret`) and `refresh_token`. RFC 6749 defines `client_secret` as a client credential, not a bearer token to validate. This is a deliberate design choice for the token exchange use case but deviates from the standard.

### Discovery Metadata

`GET /.well-known/openid-configuration` (`Endpoint.cs` lines 16–36). Present and functional. Missing: `authorization_endpoint` (correct — not implemented). Present but misleading: `response_types_supported = ["token"]`. No `userinfo_endpoint` (correct — not implemented).

### JWKS

`GET /.well-known/openid-configuration/jwks` (`Endpoint.cs` lines 38–55). Present. Returns a single RSA public key with `kty`, `use`, `kid`, `alg`, `n`, `e`. No `Cache-Control` header. Downstream consumers should cache this response; the current lack of caching headers means no guidance is provided.

### Client Validation

FACT: The Authority does not validate a `client_id` parameter on `/token` requests. There is no client registry in the Authority. The `client_credentials` grant validates the OneLogin access token (the value passed as `client_secret`). In effect, possession of a valid OneLogin access token is the only client credential required.

### Redirect URI Validation

FACT: Handled entirely by GOV.UK One Login and ASP.NET Core OIDC middleware (`Program.cs`). No repository evidence of redirect URI validation in the Authority.

### Scope Validation

FACT: No scope parameter is validated on `/token`. The discovery document advertises `ScopesSupported = ["openid"]` (`Endpoint.cs` line 31) but the `/token` endpoint does not parse or validate a scope parameter.

### State, Nonce, PKCE

- State: handled by ASP.NET Core OIDC middleware against One Login.
- Nonce: handled by ASP.NET Core OIDC middleware.
- PKCE: `UsePkce=true` (`Program.cs` line 299). Enforced in the OrganisationApp-to-OneLogin flow. The Authority `/token` endpoint has no authorization code flow, so PKCE is not applicable there.

### Token Signing Algorithm and Key Management

RS256 (`SecurityAlgorithms.RsaSha256`). `kid` derived from RFC 7638 JWK Thumbprint in production (`ConfigurationService.cs` line 34). Hardcoded fallback `kid` `"c2c3b22ac07f425eb893123de395464e"` in `AuthorityConfiguration.cs` line 32 — only reachable if `PrivateKey` is absent (which would also cause `ConfigurationService` to throw at startup). Private key supplied via environment variable / secrets management; not present in source-controlled config.

### Token Expiry and Refresh

Access token: 3600s (1 hour), configurable. Refresh token: 86400s (24 hours), configurable. Refresh tokens are opaque, single-use, PBKDF2-hashed, and stored in PostgreSQL. Refresh token revocation endpoint exists at `POST /revocation`.

### Logout / Session

`LogoutManager.cs` handles logout notification. Forwards to OneLogin logout with `sirsi-logout-api-key` header. RISK: if `OneLogin:ForwardLogoutNotificationApiKey` is empty, notification is still sent with an empty key (`?? ""`) and only `LogInformation` is emitted — not a warning or error.

### Standards Alignment Table

| Standard | Requirement | Status |
|---|---|---|
| RFC 6749 | Authorization endpoint | NOT APPLICABLE (token exchange only) |
| RFC 6749 | Token endpoint with grant type | IMPLEMENTED (non-standard usage of `client_secret`) |
| RFC 7636 | PKCE | PASS (OrganisationApp → OneLogin) |
| RFC 7517 | JWKS | PASS |
| RFC 7638 | JWK Thumbprint for `kid` | PASS |
| RFC 7009 | Token revocation (refresh) | PARTIAL (access token revocation absent; 400 on wrong hint) |
| OpenID Core | ID token | NOT IMPLEMENTED |
| OpenID Core | UserInfo endpoint | NOT IMPLEMENTED |
| OpenID Discovery | `/.well-known/openid-configuration` | PARTIAL (`response_types_supported` misleading) |
| OWASP ASVS §3.5.1 | `aud` claim and validation | PASS (conditionally — development gap exists) |

---

## 5. Authentication Assessment

### One Login Integration

GOV.UK One Login is the upstream IdP. `Program.cs` registers standard ASP.NET Core OIDC middleware. After authentication, `OidcEventsService.OnTokenValidated` fires to exchange the OneLogin access token for an Authority JWT via `TokenExchangeService`. The Authority validates the OneLogin token via OneLogin's JWKS with key refresh on failure (`TokenService.cs` validate-with-retry pattern).

### Private Key JWT Client Auth

`OidcEventsService.cs` line 111: client assertion constructed with `SecurityAlgorithms.RsaSha256` and the configured RSA key. This is the correct approach for private key JWT client authentication against One Login.

### VTR / MFA Enforcement

INFERENCE: The OIDC middleware connects to One Login which enforces VTR. No repository evidence of explicit VTR parameter configuration found in the discovery evidence provided. The Authority itself imposes no MFA requirement — it trusts the OneLogin token.

### Cookie Security

No repository evidence found in the provided discovery for explicit cookie `SameSite`, `HttpOnly`, or `Secure` flag configuration beyond ASP.NET Core defaults.

### Session Security

`SessionTimeoutInMinutes` guard added (noted in hardening context). No repository evidence from the discovery of the exact implementation location.

### Error Handling and Account Enumeration

FACT: The `/token` endpoint returns HTTP 400 with `"Invalid grant type"` for unsupported grant types. No evidence of user enumeration risk in the Authority — it validates tokens, not usernames/passwords.

### Audit Logging

`LoggerMessage.Define` auth events added to `TokenService` in the hardening sprint. `AuditLog` collection exists (`app_registry_audit_logs`) with `EntityType`, `EntityId`, `Action`, `UserId`, `Timestamp`. The `sirsi-logout-api-key` empty-key condition logs `LogInformation` only — should be elevated to `LogWarning`.

---

## 6. Authorisation and Role Model Assessment

### User Model

FACT: There is no `User` entity or User MongoDB collection. Users are represented as an opaque `UserPrincipalId` string (the `sub` URN from One Login) on `UserOrganisationMembership` and `UserApplicationAssignment`. No display name, email, or profile data is stored. The `AssignUser` page renders `@member.UserPrincipalId (@member.OrganisationRole)` — raw URN, not a human name.

### Application Model

`Application` entity: `Id` (Guid), `Name`, `ClientId`, `Description`, `Category`, `IsActive`, with embedded `Permissions` and `Roles` arrays. Stored in MongoDB (`app_registry` database).

### Role Model

`ApplicationRole`: scoped to an `Application` via `ApplicationId`. `IsActive` soft-delete. Embedded `RolePermissions` join array. Role names are free strings — no enumerated type or validation constraint.

### Permission Model

`ApplicationPermission`: scoped to an `Application`. No `IsActive` flag — deletion is a hard `PullFilter` remove. Dangling references in `RolePermission` after deletion are not cleaned up.

### User-to-Application Assignment Model

`UserApplicationAssignment`: one document per `(UserPrincipalId, ApplicationId, OrganisationId)` triple. Unique index `idx_userassignment_unique` enforces no duplicate assignments. `IsActive` soft-delete. No `ExpiresAt` — assignments are permanent until explicitly revoked.

### User-to-Application-Role Model

`UserRoleAssignment`: embedded in `UserApplicationAssignment`. Stores `RoleId` only. No validation that the `RoleId` belongs to the same application as the parent `UserApplicationAssignment`. Cross-application role assignment is possible at the data layer without error.

### Are Roles Application-Scoped?

Yes, definitively. `ApplicationRole.ApplicationId` is a required FK. `GetRolesAsync` filters exclusively within one application document. `HasApplicationRole` in `ClaimService.cs` lines 70–81 requires both `organisationId` and `clientId` to match before returning true. A role from Application A cannot match a check for Application B.

### Backend Enforcement vs UI Assignment

UI assignment (OrgAdmin) operates on `UserApplicationAssignment` — a write to MongoDB via `POST /api/organisations/{orgId}/applications/{appId}/users`. Backend enforcement (`HasApplicationRole`) reads from `cdp_claims` in the JWT — a static snapshot taken at login time. Changes made via the UI take effect only at the user's next sign-in (as stated in the success notification). There is no mechanism to force immediate token invalidation when role assignments change.

### Auth Policies

Three policies (`AuthorizationPolicies.cs`):
- `PlatformAdmin`: requires `platform_role=admin` claim. Flat, not org-scoped. Bypasses all org checks.
- `OrgAdmin`: requires `org:{orgId}:role` claim with value `Admin` or `Owner` (case-insensitive). Reads `orgId` from route. `OrganisationRoleHandler` inspects token claims only — no DB lookup.
- `OrgMember`: same mechanism; accepts `Member`, `Admin`, or `Owner`.

CRITICAL FACT: No code path in `CreateAccessToken` writes `org:{orgId}:role` claims into the token. These claims are never present in an Authority-issued JWT. `OrgAdmin` and `OrgMember` policies will deny all non-platform-admin users.

FACT: The following endpoints have no `.RequireAuthorization` call and are publicly accessible: `GET /api/applications/{appId}`, `GET /api/applications/{appId}/roles`, `GET /api/applications/{appId}/permissions`, `GET /api/organisations/slug/{slug}`.

---

## 7. Claim Assignment and Token Issuing Assessment

### Exact Claims in Issued JWT

`TokenService.CreateAccessToken` (`TokenService.cs` lines 173–219) issues:
- `sub` — user URN (`JwtClaimTypes.Subject`)
- `channel` — hardcoded `"one-login"`
- `roles` — comma-joined `PersonScope` values (platform-level: `SUPERADMIN`, `SUPPORTADMIN`, etc.)
- `cdp_claims` — JSON blob (conditional on `ClaimsApiEnabled=true`; `JsonClaimValueTypes.Json`)
- `iss` — `config.Issuer`
- `aud` — `config.Issuer` (same value)
- `exp`, `iat`, `nbf` — standard timestamps
- `kid` — on token header

### `cdp_claims` Shape

```json
{
  "userPrincipalId": "<urn>",
  "organisations": [
    {
      "organisationId": "<guid>",
      "organisationName": "<string>",
      "organisationRole": "<string>",
      "applications": [
        {
          "applicationId": "<guid>",
          "applicationName": "<string>",
          "clientId": "<string>",
          "roles": ["<roleName>"],
          "permissions": ["<permName>"]
        }
      ]
    }
  ]
}
```

Sourced from `GET /organisations/claims/users/{urn}` backed by `GetUserClaimsUseCase`. Note: this is a separate use case from `GetClaimsTreeUseCase`.

### Application Role Scoping Analysis

PASS. `HasApplicationRole` (`ClaimService.cs` lines 70–81) requires exact `Guid` match on `organisationId` and exact `string` match on `clientId` before checking role name. Roles are structurally isolated per application per organisation in the `cdp_claims` JSON. Cross-application leakage is not possible via this path.

### Audience Scoping Analysis

The `aud` claim is set to the Authority's own issuer URL (`Audience = config.Issuer`, `TokenService.cs` line 213). The Authority is both issuer and intended audience. If downstream resource servers validate `aud`, they must be configured to accept the Authority's issuer URL as a valid audience — which is semantically unusual. OPINION: `aud` should ideally be set to the resource server's identifier, not the issuer.

### Role Leakage Risk Assessment

LOW. The `cdp_claims` structure separates roles by `organisationId` + `clientId`. `ClaimService.HasApplicationRole` and `HasApplicationPermission` both enforce the two-key filter. No role from Application A can be returned by a check for Application B.

### GetClaimsTreeUseCase vs GetUserClaimsUseCase — Which is Called?

`TokenService` calls `GetUserClaimsUseCase` (via HTTP `GET /organisations/claims/users/{urn}`). `GetClaimsTreeUseCase` is a separate use case with its own repository dependencies (`IOrganisationRepository`, `IUserAssignmentRepository`). There is no code path from `TokenService` to `GetClaimsTreeUseCase`. The Application Registry's MongoDB role assignments are therefore surfaced in tokens only if `GetUserClaimsUseCase` reads from the same MongoDB collections — which must be verified independently.

### Token Validation Readiness

- Signature validation: PASS (`AddJwtBearerAuthentication` + JWKS endpoint).
- `aud` validation: PASS (consumer side, `ValidateAudience=true`).
- `iss` validation: PASS (standard JWT bearer middleware).
- `exp` validation: PASS (standard).
- `roles` and `cdp_claims` extraction: PASS via `ClaimService`.
- `org:{orgId}:role` claims: FAIL — never written into token.

### Token Test Coverage

`TokenServiceTest.cs`: 10 `[Fact]` methods. All passing. Key gaps: no test for expired refresh token DB record; no test for valid-format token with no DB record; no assertion that revocation actually marks the record as `Revoked=true` in the repository.

---

## 8. Application Import / Registration Assessment

### Is There a UI to Register New Applications?

FACT: No. There is no frontend page in `CO.CDP.OrganisationApp` or any other Frontend project that calls `POST /api/applications`. No `PlatformAdmin` UI directory exists under `Frontend/`. The `IAppRegistryClient` interface (`Frontend/CO.CDP.OrganisationApp/WebApiClients/IAppRegistryClient.cs`) exposes no method for creating, updating, or deleting an Application, Role, or Permission.

### API Endpoint for Registration

`POST /api/applications` — `ApplicationEndpoints.cs`. Requires `PlatformAdmin` policy. Returns HTTP 201 with `Location` header.

### Required Fields

`Application`: `Name` (required), `ClientId` (required). `Description` and `Category` are optional.

### Validation

INFERENCE: Standard model validation via ASP.NET Core. No repository evidence of explicit duplicate `ClientId` checks at the application layer beyond what the test coverage reveals. The `ApplicationEndpointsTests.cs` tests only assert authorization (201/403) — no duplicate detection test is evident in the discovery.

### Duplicate Handling

No repository evidence found of a unique index on `ClientId` in the MongoDB schema or explicit duplicate-check logic in `MongoApplicationRepository.CreateApplicationAsync`. This is a gap: two applications with the same `ClientId` could be created, which would break the two-key role lookup in `HasApplicationRole`.

### Audit Trail

`AuditLog` collection (`app_registry_audit_logs`) records creation events via `ICurrentUserContext`. Auditing is present at the repository layer.

### Security Risks

The `POST /api/applications` endpoint is `PlatformAdmin`-gated. Risk: no UI means no workflow control — any bearer token with `platform_role=admin` can register arbitrary applications. No rate limiting or additional approval workflow is evident.

### What is Missing

1. UI for application registration (PlatformAdmin portal or CLI tooling).
2. Unique index on `ClientId` to prevent duplicate application registration.
3. Explicit duplicate detection with a meaningful error response.
4. UI for enabling an application to an organisation (the `POST /api/organisations/{orgId}/applications/{appId}` endpoint exists but has no corresponding frontend form).

---

## 9. UI Capability Assessment

### All Application Registry UI Pages

| Page | Route | Auth Guard |
|---|---|---|
| ApplicationList | `/organisation/{id}/applications` | `OrgScopeRequirement.Admin` + `IsBuyer()` |
| ApplicationDetail | `/organisation/{id}/applications/{appId:guid}` | `OrgScopeRequirement.Admin` + `IsBuyer()` |
| UserAssignments | `/organisation/{id}/applications/{appId:guid}/user-assignments` | `OrgScopeRequirement.Admin` + `IsBuyer()` |
| AssignUser (new + edit) | `/organisation/{id}/applications/{appId:guid}/user-assignments/assign` | `OrgScopeRequirement.Admin` + `IsBuyer()` |
| UserAssignmentCheckAnswers | `/organisation/{id}/applications/{appId:guid}/user-assignments/check-answers` | `OrgScopeRequirement.Admin` + `IsBuyer()` |
| RevokeConfirmation | `/organisation/{id}/applications/{appId:guid}/user-assignments/{userId}/revoke` | `OrgScopeRequirement.Admin` + `IsBuyer()` |

Entry point: `OrganisationOverview.cshtml` lines 364–373, gated to `OrgScopeRequirement.Admin` + `IsBuyer()`.

### What Works End-to-End

- List applications enabled for an organisation.
- View application detail including roles and permissions breakdown.
- Assign a member to an application with one or more roles (check-and-confirm pattern).
- Edit roles for an existing assignment.
- Revoke a user's application access (confirm-then-delete pattern).
- GOV.UK success notification banner after each write operation.
- Client-side user search/filter on the UserAssignments list.

### Auth Guards

All six pages: `[Authorize(Policy = OrgScopeRequirement.Admin)]` + in-handler `org.IsBuyer()` check. Non-buyer organisations and non-admin users are blocked. CRITICAL: because `org:{orgId}:role` claims are not written into the Authority JWT, `OrgScopeRequirement.Admin` will deny all real OrgAdmin users. Only `PlatformAdmin` users bypass this.

### Missing Pages or Flows

1. Platform Admin UI: register application, manage roles, manage permissions, enable/disable org-application links — no pages exist.
2. Application enable toggle from OrgAdmin perspective — OrgAdmin can see applications already linked but cannot link new ones via UI.
3. Search/filter on ApplicationList.
4. Display of resolved user names (only raw URN shown in dropdowns).
5. Token/claims preview showing what access a user will have after assignment.

### Test Coverage

- E2E: 3 navigation tests + 7 functional tests (`ApplicationRegistryNavigationTests.cs`, `ApplicationRegistryFunctionalTests.cs`). Tests degrade gracefully (skip) in empty environments. Cannot run in CI without seeded data and a running API + frontend.
- Unit: `AssignUserModel`, `RevokeConfirmationModel`, `ApplicationListModel`, `ApplicationDetailModel` page models are not directly unit-tested by name in the discovered test files — coverage is provided by E2E tests only.

### Gaps

- Page model unit tests do not exist — all frontend coverage is E2E-only.
- E2E tests require live seeded data; CI runs on blank database yield no assertions.
- No test covers the `IsEdit=true` (edit-roles) path end-to-end.
- No test covers the scenario where `GetOrganisationApplicationsAsync` fails — the silent empty-list fallback is not tested.

---

## 10. Client Application Consumption Assessment

### OIDC Discovery Endpoint

`GET /.well-known/openid-configuration` — present, functional. A client developer should note: `ResponseTypesSupported=["token"]` is advertised but no authorization endpoint exists. Treat the Authority purely as a token exchange service, not a full OIDC provider.

### JWKS Endpoint

`GET /.well-known/openid-configuration/jwks` — present. Single RSA key. No `Cache-Control` headers — clients should implement their own caching (recommend 1-hour TTL). Key ID (`kid`) is RFC 7638-derived.

### Audience Validation Readiness

Consuming services must validate `aud` against the Authority issuer URL (e.g. `http://authority:8092` in development). `AddJwtBearerAuthentication` configures this. `ValidateAudience=true` is set. This is ready.

### Claims Shape Usability

Platform-level roles are in `roles` (comma-delimited string). Application-specific roles and permissions are in `cdp_claims` (JSON, requires `JsonConvert.DeserializeObject<UserClaims>` or equivalent). The nested structure (`organisations[].applications[].roles`) means two lookups are required to check a role: filter by `organisationId`, then by `clientId`. `ClaimService` provides `HasApplicationRole(Guid orgId, string clientId, string roleName)` as a ready-to-use helper.

### Role Extraction Pattern

```csharp
// Inject IClaimService; call:
bool hasRole = claimService.HasApplicationRole(orgId, clientId, "Reader");
UserClaims? claims = claimService.GetApplicationClaims();
```

`HasApplicationRole` and `HasApplicationPermission` are pure in-memory — no DB call. Safe to call on every request.

### Reject Invalid Token Capability

`AddJwtBearerAuthentication` validates: RS256 signature (via JWKS), `iss`, `aud`, `exp`, `nbf`. Tampered tokens will be rejected at the middleware layer before reaching any endpoint handler.

### What a Client Developer Needs to Know

1. The Authority is a token exchange service, not a standard OIDC Authorization Server. Do not redirect users to it for login.
2. `cdp_claims` is present only when `ClaimsApiEnabled=true` and the Organisation API is reachable at token-issuance time. Code defensively — always check for null before reading `cdp_claims`.
3. Role checks require both `organisationId` (Guid) and `clientId` (string) — neither alone is sufficient.
4. Role changes in the Application Registry take effect at next sign-in, not immediately.
5. The `aud` claim equals the Authority issuer URL. Configure JWT Bearer middleware with `ValidAudience = "<authority-base-url>"`.
6. JWKS has no caching headers — implement your own cache.

---

## 11. Functional Test Plan

### Test 1: Application Registration — Happy Path
**Purpose:** Verify PlatformAdmin can register a new application with required fields.
**Preconditions:** PlatformAdmin JWT issued; no existing application with the target `clientId`.
**Steps:** POST `/api/applications` with `{"name":"TestApp","clientId":"test-app-001"}`. Check response.
**Expected Result:** HTTP 201; `Location` header set; application appears in `GET /api/applications`.
**Repository Support:** `ApplicationEndpointsTests.cs` — partial (auth gating tested; content assertion needed).
**Automation Candidate:** Yes (integration test).
**Priority:** HIGH.

### Test 2: Application Registration — Duplicate ClientId
**Purpose:** Verify that registering a second application with the same `clientId` is rejected.
**Preconditions:** Application with `clientId=test-app-001` already exists.
**Steps:** POST `/api/applications` with `{"name":"DuplicateApp","clientId":"test-app-001"}`.
**Expected Result:** HTTP 409 or HTTP 422 with descriptive error.
**Repository Support:** No repository evidence found of this being tested or uniqueness being enforced.
**Automation Candidate:** Yes — currently UNTESTABLE (no unique index evidence).
**Priority:** HIGH.

### Test 3: Application Registration — Missing Required Fields
**Purpose:** Verify validation rejects incomplete payloads.
**Steps:** POST `/api/applications` with `{}` (no name, no clientId).
**Expected Result:** HTTP 400 with validation errors for `name` and `clientId`.
**Repository Support:** Not explicitly tested in discovered test files.
**Automation Candidate:** Yes.
**Priority:** MEDIUM.

### Test 4: User-to-Application Assignment — Assign User
**Purpose:** Verify OrgAdmin can assign an org member to an application with a role.
**Preconditions:** Organisation has `IsActive=true`, at least one enabled application, at least one active member.
**Steps:** POST `/api/organisations/{orgId}/applications/{appId}/users` with `{"userPrincipalId":"<urn>","roleIds":["<roleId>"]}`.
**Expected Result:** HTTP 201; assignment retrievable via GET.
**Repository Support:** `UserAssignmentEndpointsTests.cs` — covered.
**Automation Candidate:** Yes.
**Priority:** HIGH.

### Test 5: User-to-Application Assignment — Remove Assignment
**Purpose:** Verify OrgAdmin can revoke a user's application access.
**Steps:** DELETE `/api/organisations/{orgId}/applications/{appId}/users/{userId}`.
**Expected Result:** HTTP 204; `GetAssignmentAsync` returns `IsActive=false`.
**Repository Support:** `UserAssignmentEndpointsTests.cs` — covered.
**Automation Candidate:** Yes.
**Priority:** HIGH.

### Test 6: User-to-Application Assignment — Inactive User
**Purpose:** Verify that a soft-deleted membership cannot be assigned to an application.
**Preconditions:** User membership `IsActive=false`.
**Steps:** POST `/api/organisations/{orgId}/applications/{appId}/users` referencing the inactive user.
**Expected Result:** HTTP 400 or 404.
**Repository Support:** No repository evidence found.
**Automation Candidate:** Yes.
**Priority:** MEDIUM.

### Test 7: Role Assignment — Role in App A vs App B
**Purpose:** Verify a user can hold different roles in two different applications within the same organisation.
**Steps:** Assign User X to App A as "Reader". Assign User X to App B as "Admin". GET both assignments.
**Expected Result:** Two separate `UserApplicationAssignment` documents; different `UserRoleAssignment` lists.
**Repository Support:** Data model supports this; `GetClaimsTreeUseCaseTests.cs` covers the correct-roles path.
**Automation Candidate:** Yes.
**Priority:** HIGH.

### Test 8: Role Assignment — Remove Specific Role
**Purpose:** Verify PUT (replace role set) removes a role without removing the assignment.
**Steps:** PUT `/api/organisations/{orgId}/applications/{appId}/users/{userId}` with `{"roleIds":["<newRoleId>"]}` (removing an old role).
**Expected Result:** HTTP 204; GET shows updated role list; old role absent.
**Repository Support:** `UserAssignmentEndpointsTests.cs` — auth-gating tested; content change assertion needed.
**Automation Candidate:** Yes.
**Priority:** MEDIUM.

### Test 9: OIDC Login — Full Flow
**Purpose:** Verify end-to-end login via One Login results in an Authority JWT with `sub`, `cdp_claims`, `roles`.
**Preconditions:** Live One Login integration environment; test user with org membership and application assignment.
**Steps:** Navigate to OrganisationApp; authenticate via One Login; inspect issued JWT.
**Expected Result:** Cookie set; JWT contains `sub`, `aud=<issuer>`, `cdp_claims` with organisation and application data, `roles`.
**Repository Support:** `TokenServiceTest.cs` unit-tests claim injection; E2E tests cover navigation post-login.
**Automation Candidate:** E2E (Playwright) — partially covered.
**Priority:** HIGH.

### Test 10: OIDC Login — Bad Client Credentials
**Purpose:** Verify Authority rejects a token exchange request with an invalid OneLogin access token.
**Steps:** POST `/token` with `grant_type=client_credentials`, `client_secret=<invalid>`.
**Expected Result:** HTTP 400.
**Repository Support:** `TokenServiceTest.cs` — `ValidateOneLoginToken` with invalid key path covered.
**Automation Candidate:** Yes.
**Priority:** HIGH.

### Test 11: OIDC Login — Bad Scope
**Purpose:** Verify Authority handles unknown scope gracefully.
**Steps:** POST `/token` with `grant_type=client_credentials`, `scope=unknown_scope`, valid `client_secret`.
**Expected Result:** Token issued without error (scope is not validated by Authority); `roles` claim reflects actual DB scopes.
**Repository Support:** No scope validation evidence — scope parameter is ignored.
**Automation Candidate:** Yes.
**Priority:** LOW.

### Test 12: Token Validation — `aud` Claim
**Purpose:** Verify consuming API rejects a token with a wrong `aud`.
**Steps:** Issue a JWT with `aud=https://attacker.example.com`. Present to Organisation WebApi.
**Expected Result:** HTTP 401.
**Repository Support:** `AddJwtBearerAuthentication` enforces `ValidateAudience=true`. No explicit test found in discovered files.
**Automation Candidate:** Yes (unit test of bearer middleware configuration).
**Priority:** HIGH.

### Test 13: Token Validation — Expired Token
**Purpose:** Verify consuming API rejects an expired JWT.
**Steps:** Issue a JWT with `exp` in the past. Present to Organisation WebApi.
**Expected Result:** HTTP 401.
**Repository Support:** Standard middleware behaviour; no explicit test found.
**Automation Candidate:** Yes.
**Priority:** MEDIUM.

### Test 14: Token Validation — Tampered Signature
**Purpose:** Verify consuming API rejects a JWT with a modified payload and original signature.
**Steps:** Decode JWT; modify `roles` claim; re-encode without resigning. Present to Organisation WebApi.
**Expected Result:** HTTP 401.
**Repository Support:** Standard RS256 validation; no explicit test found.
**Automation Candidate:** Yes.
**Priority:** HIGH.

### Test 15: Role Leakage — Roles from App A Not Present for App B
**Purpose:** Verify `HasApplicationRole` with App B's `clientId` does not return roles assigned only in App A.
**Steps:** Issue token with `cdp_claims` containing App A role "Admin" under org X. Call `HasApplicationRole(orgX, "app-b-client-id", "Admin")`.
**Expected Result:** `false`.
**Repository Support:** `ClaimServiceTests.cs` — "false when wrong clientId" covered.
**Automation Candidate:** Yes — already tested.
**Priority:** HIGH.

### Test 16: Client Consumption — Extract Roles and Enforce Access
**Purpose:** Verify `ClaimService.HasApplicationRole` returns correct result for authenticated user.
**Steps:** Issue token with `cdp_claims` containing role "Reader" for App A. Call endpoint requiring "Admin" for App A.
**Expected Result:** HTTP 403.
**Repository Support:** `ClaimServiceTests.cs` — role match/mismatch covered.
**Automation Candidate:** Yes.
**Priority:** HIGH.

### Test 17: Client Consumption — Reject Tampered `cdp_claims`
**Purpose:** Verify that a tampered `cdp_claims` JSON does not bypass role checks.
**Steps:** Modify `cdp_claims` claim in JWT payload; present to API.
**Expected Result:** HTTP 401 (signature validation fails before claims are read).
**Repository Support:** Standard RS256 middleware.
**Automation Candidate:** Yes.
**Priority:** HIGH.

---

## 12. Test Execution Results

### Build Status

```
dotnet build — Exit code: 0
31 Warning(s), 0 Error(s)
Time Elapsed: 00:00:34.47
```

Notable warning: `OrganisationFindAndApply.cshtml.cs(15,25): warning CS9113: Parameter 'organisationClient' is unread.` — dead parameter, not a functional issue.

### Unit Test Results (per project)

| Project | Passed | Failed | Skipped | Duration |
|---|---|---|---|---|
| CO.CDP.Organisation.Authority.Tests | 21 | 0 | 0 | 904ms |
| CO.CDP.Authentication.Tests | 146 | 0 | 0 | 141ms |
| CO.CDP.Organisation.WebApi.Tests (all) | 616 | 0 | 0 | 42s |

All three projects: **GREEN**.

### ApplicationRegistry-Specific Test Results

`CO.CDP.Organisation.WebApi.Tests` filtered to `ApplicationRegistry`: **69 passed, 0 failed, 0 skipped** (9s). This covers endpoint tests for all Application, Organisation, UserAssignment, and Claims endpoints plus 6 `GetClaimsTreeUseCase` unit tests.

### E2E Test Status

- Files: `ApplicationRegistryNavigationTests.cs` (3 tests) + `ApplicationRegistryFunctionalTests.cs` (7 tests).
- All page objects exist and project builds.
- Structural state: runnable. Runtime state: requires live API, live frontend, and seeded MongoDB application data.
- Most functional tests call `Assert.Ignore` when no seeded data is present — CI against a blank database yields 0 assertions from 5 of 7 functional tests.
- Status: **STRUCTURALLY PASS / FUNCTIONALLY UNTESTED IN CI**.

### Missing Test Coverage (quantified)

| Gap | Count | Severity |
|---|---|---|
| Refresh token expiry path (expired DB record) | 1 missing test | MEDIUM |
| Refresh token valid-format, no DB record | 1 missing test | MEDIUM |
| Refresh token revocation side-effect (Verify mock) | 1 missing assertion | MEDIUM |
| `HasApplicationPermission` wrong orgId + wrong clientId | 2 missing tests | LOW |
| `GetClaimsTreeUseCase` with disabled application | 1 missing test | MEDIUM |
| Application endpoint update/delete content assertions | 4 missing assertions | MEDIUM |
| `GET /api/claims/{urn}` — user not found (null return) | 1 missing test | LOW |
| Duplicate `clientId` on application creation | 1 missing test | HIGH |
| `aud` validation rejection test | 1 missing test | HIGH |
| Tampered JWT rejection test | 1 missing test | HIGH |
| Org-scoped role claims absent from token | 1 missing test (demonstrates the gap) | CRITICAL |
| MongoDB repository integration tests | 0 (none exist) | HIGH |

**Total identified gaps: 16 distinct test scenarios.**

### Commands

```bash
dotnet build /Users/sammywilliams/RiderProjects/CO-CDP/CO.CDP.sln
dotnet test Services/CO.CDP.Organisation.Authority.Tests
dotnet test Libraries/CO.CDP.Authentication.Tests
dotnet test Services/CO.CDP.Organisation.WebApi.Tests
dotnet test Services/CO.CDP.Organisation.WebApi.Tests --filter "FullyQualifiedName~ApplicationRegistry"
```

---

## 13. Critical Gaps and Unspoken Truth

### Gap 1: Org-Scoped Role Claims Never Written Into the Token

**Statement:** `OrgAdmin` and `OrgMember` authorization policies require `org:{orgId}:role` claims in the JWT. These claims are never written by `TokenService.CreateAccessToken`.

**Evidence:** `TokenService.cs` lines 173–219 — only `sub`, `channel`, `roles`, `cdp_claims` are written. `OrganisationRoleHandler` reads `org:{orgId}:role` from token claims only — no DB lookup.

**Why it matters:** Every OrgAdmin-gated endpoint (member management, user assignment reads/writes) will return HTTP 403 for all non-PlatformAdmin users. The UI pages for user assignment are effectively inaccessible to their intended users.

**Production risk:** CRITICAL.

**Required fix:** Enrich `CreateAccessToken` with `org:{orgId}:role` claims derived from `OrganisationPerson` membership records, or restructure `OrganisationRoleHandler` to perform a DB lookup against `UserOrganisationMembership`.

**Test to prove closure:** Assert that a JWT issued for a user with `OrganisationRole=Admin` in org X contains the claim `org:{X}:role=Admin`. Assert that `OrgAdmin` policy succeeds for a request to `GET /api/organisations/{X}/members` using that token.

### Gap 2: No Platform Admin UI

**Statement:** Application registration, role/permission management, and org-application linking have no frontend. Operations must be performed via direct API calls.

**Evidence:** `IAppRegistryClient.cs` — no create/update/delete methods. No `PlatformAdmin` UI directory under `Frontend/`.

**Why it matters:** The platform cannot be operated by non-technical users. All application onboarding requires API access and knowledge of MongoDB data structures.

**Production risk:** HIGH.

**Required fix:** Build a Platform Admin portal or provide documented CLI/API tooling with access control.

**Test to prove closure:** E2E test navigating to PlatformAdmin UI, registering an application, defining roles, and enabling it for an organisation.

### Gap 3: Audience Validation Disabled in Development Without a Config Guard

**Statement:** In local development, `OneLogin:ClientId` is absent from `appsettings.Development.json`. `ValidateAudience=false` is set silently — any OneLogin-signed token is accepted.

**Evidence:** `TokenService.cs` lines 103–107; `appsettings.Development.json` — no `OneLogin:ClientId`.

**Why it matters:** A developer running locally with a stolen or replayed OneLogin token from another client could authenticate against the Authority without audience rejection. Development habits can embed bad assumptions.

**Production risk:** HIGH (in development; LOW in production if `OneLoginClientId` is always configured via secrets).

**Required fix:** Add `IStartupFilter` or `WebApplication.Services.ValidateOnStart()` assertion: throw if `OneLogin:ClientId` is not configured.

**Test to prove closure:** Assert that Authority startup fails with a descriptive error when `OneLogin:ClientId` is missing.

### Gap 4: No Unique Index on `Application.ClientId`

**Statement:** Two applications with the same `clientId` can be created. `HasApplicationRole` uses `clientId` as a lookup key — duplicates would cause false positive matches.

**Evidence:** No repository evidence of a unique index on `ClientId` in `MongoApplicationRepository` or schema definitions.

**Why it matters:** Role checks in `ClaimService.HasApplicationRole` match on `clientId` string equality. If two applications share a `clientId`, a user assigned a role in one may satisfy role checks for the other.

**Production risk:** MEDIUM (requires deliberate duplicate creation by a PlatformAdmin).

**Required fix:** Add a MongoDB unique index on `Application.ClientId`. Add duplicate-check logic in `CreateApplicationAsync` with HTTP 409 response.

**Test to prove closure:** Attempt to POST two applications with the same `clientId`; assert HTTP 409 on second request.

### Gap 5: `OrganisationApplication` Disable is Irreversible at the Document Level

**Statement:** Disabling an org-application link physically removes the `OrganisationApplication` embedded document via `PullFilter`. There is no `IsEnabled` flag, no `DisabledAt`, no historical state queryable from the document.

**Evidence:** `MongoOrganisationRepository.DisableApplicationAsync` uses `PullFilter` (described in data model report).

**Why it matters:** Compliance and audit requirements may require knowing which applications were historically enabled for an organisation. This information is recoverable only from audit logs, not from the primary data store.

**Production risk:** MEDIUM.

**Required fix:** Add `IsEnabled` (bool), `DisabledAt` (DateTimeOffset?), `DisabledBy` (string?) to `OrganisationApplication`. Change disable to a soft-update.

**Test to prove closure:** After disabling, assert the embedded document still exists with `IsEnabled=false` and `DisabledAt` set.

### The Uncomfortable Truth (plainly stated)

The feature/app-roles branch delivers a well-structured data model and a competent set of API endpoints. The hardening sprint addressed real security issues. But the entire OrgAdmin assignment workflow — the feature's primary user-facing capability — is locked behind authorization policies that will never succeed for its intended users. The success notification "Changes take effect when the user next signs in" is a UX lie: even after next sign-in, the org-scoped role claims are absent from the token, so `OrgAdmin` endpoints remain forbidden. The feature cannot be deployed to production and used by any real OrgAdmin until the token enrichment gap is closed.

---

## 14. Implementation Backlog

| Priority | Title | Description | Acceptance Criteria | Files | Test Requirement | Compliance Rationale |
|---|---|---|---|---|---|---|
| P0 | Write org-scoped role claims into Authority JWT | `CreateAccessToken` must write `org:{orgId}:role` claims for each `OrganisationPerson` membership record | JWT issued for a user with OrgAdmin role contains `org:{orgId}:role=Admin`; `OrgAdmin` policy passes | `TokenService.cs` | Unit test: assert claim present; integration test: OrgAdmin endpoint returns 200 | OrgAdmin/OrgMember policies non-functional without this |
| P0 | Add `OneLogin:ClientId` startup validation guard | Startup must fail with a descriptive error if `OneLogin:ClientId` is absent | Authority throws `InvalidOperationException` at startup if `OneLoginClientId` is null/empty | `ConfigurationService.cs` or `Program.cs` | Unit test: missing config → exception | OWASP ASVS §9.2.1 — audience validation must not be silently bypassed |
| P1 | Build Platform Admin UI for application management | Create Razor pages for: register application, define roles, define permissions, enable/disable org-application link | PlatformAdmin can complete full application onboarding without API tooling | New pages under `Frontend/CO.CDP.OrganisationApp/Pages/Admin/` | E2E test: create app → add role → enable for org | Operational requirement |
| P1 | Add unique index on `Application.ClientId` | Prevent duplicate `clientId` values in MongoDB; return HTTP 409 on conflict | POST with duplicate `clientId` returns 409; unique index enforced in `MongoAppRegistryDatabase` | `MongoApplicationRepository.cs`, `MongoAppRegistryDatabase.cs` | Integration test: duplicate POST → 409 | Data integrity; role isolation correctness |
| P1 | Validate `UserRoleAssignment.RoleId` belongs to correct application | `AssignUserAsync` must verify the supplied `RoleId` belongs to the `ApplicationId` of the assignment | Attempt to assign role from App B to a user assignment in App A returns HTTP 400 | `MongoUserAssignmentRepository.cs` or use-case layer | Unit test: cross-application roleId → error | Data integrity; authorization correctness |
| P2 | Soft-delete `OrganisationApplication` disable | Replace `PullFilter` with `IsEnabled=false`, `DisabledAt`, `DisabledBy` update | After disable, embedded document exists with `IsEnabled=false`; history queryable | `MongoOrganisationRepository.cs`, `OrganisationApplication` entity | Unit test: disable → document still present with flags set | Audit and compliance |
| P2 | Add `IsActive` filter to all `GetByIdAsync` methods | `GetApplicationByIdAsync`, `GetRoleByIdAsync`, `GetOrganisationByIdAsync` must exclude inactive records | GET for a soft-deleted entity by ID returns HTTP 404 | `MongoApplicationRepository.cs`, `MongoOrganisationRepository.cs` | Unit test: GetById on inactive entity → null | Security: prevent assignment to soft-deleted entities |
| P2 | Add missing refresh token tests | Cover: expired DB record; valid-format token with no DB record; assert revocation side-effect | 3 new [Fact] methods; all passing | `TokenServiceTest.cs` | Unit tests | Test coverage completeness |
| P2 | Add `Cache-Control` headers to JWKS endpoint | Add `Cache-Control: public, max-age=3600` to `GET /.well-known/openid-configuration/jwks` response | Response includes `Cache-Control: public, max-age=3600` | `Endpoint.cs` | Integration test: assert response header | Performance; downstream consumer guidance |
| P2 | Add cascade logic for permission deletion | When `DeletePermissionAsync` removes a permission, remove all `RolePermission` records referencing it | After permission delete, no `RolePermission` with that `PermissionId` exists | `MongoApplicationRepository.cs` | Unit test: delete permission → role no longer contains it | Data integrity |
| P3 | Standardise identity type across all entities | Replace Guid `GrantedBy`/`UpdatedBy`/`AssignedBy` with string URN (consistent with `AssignedBy` and `AuditLog.UserId`) | All "who did this" fields use `string` (URN); no Guid identity fields | All entity classes in `ApplicationRegistry/Persistence/` | Schema migration + unit tests | Data consistency; cross-entity join capability |
| P3 | Resolve user display names in frontend dropdowns | Replace raw URN in `AssignUser` member dropdown with display name resolved from an identity store or Organisation API profile | Dropdown shows "John Smith (Admin)" not "urn:fdc:gov.uk:2022:xxx (Admin)" | `AssignUserModel.cshtml` | E2E test: assert readable names in dropdown | Usability |
| P3 | Add RFC 7009-compliant behaviour to `/revocation` for unknown token type hints | Return HTTP 200 (not 400) for unsupported `token_type_hint` values per RFC 7009 §2.2 | POST `/revocation` with `token_type_hint=access_token` returns 200 | `Endpoint.cs` | Unit test: unsupported hint → 200 | RFC 7009 §2.2 compliance |
| P3 | Add MongoDB integration tests for ApplicationRegistry repositories | Cover `MongoApplicationRepository`, `MongoOrganisationRepository`, `MongoUserAssignmentRepository` with a real MongoDB instance (Testcontainers) | At least 1 integration test per repository covering create, read, update, soft-delete | New test project or new test class using `Testcontainers.MongoDb` | Integration tests (Testcontainers) | Confidence in persistence layer; index verification |

---

## 15. Final Recommendation

### Classification: Architecturally Incomplete

**Justification:**

The feature/app-roles branch has delivered the structural foundations of an Application Registry: a well-designed MongoDB data model with application-scoped roles, a working REST API, a GOV.UK-compliant frontend for OrgAdmin user assignment, a tested token exchange service, and a meaningful hardening sprint that closed several real security issues (PKCE, `aud` emission, RFC 7638 `kid`, PBKDF2 refresh tokens, real caller identity).

However, the feature cannot function for its primary intended users — OrgAdmins managing application access — because of a fundamental architectural gap: the authorization policies `OrgAdmin` and `OrgMember` read `org:{orgId}:role` claims from the JWT, but no code path in the Authority's token issuance writes these claims. This is not a minor implementation detail. It is the bridge between the identity layer and the application registry that does not exist. Until this bridge is built, every write operation by a real OrgAdmin (assigning users, editing roles, revoking access) will return HTTP 403.

The missing Platform Admin UI is a second architectural gap: the system has no operational path for non-engineers to register new applications, define roles, or link applications to organisations. These capabilities exist only via direct API calls.

These two gaps together mean the feature cannot support a production workload. It is not ready for production, not merely for quality reasons but because the primary user journey — an OrgAdmin managing who has access to what applications — is blocked at the authorization layer.

**Evidence summary:**
- `OrganisationRoleHandler` reads `org:{orgId}:role` from token; `TokenService.cs` lines 173–219 never writes these claims. FACT.
- `IAppRegistryClient.cs` exposes no application create/update/delete methods. No PlatformAdmin UI directory exists. FACT.
- 69 ApplicationRegistry tests pass. All three test assemblies: GREEN. FACT.
- E2E tests structurally valid but data-dependent; CI runs yield zero functional assertions. FACT.
- 16 identified test coverage gaps, including 2 HIGH-severity gaps (duplicate `clientId` validation; `aud` rejection). FACT.

**Path to production:**
1. Close P0 gap: write `org:{orgId}:role` claims into the Authority JWT (or restructure policy handler to perform a DB lookup).
2. Add `OneLogin:ClientId` startup guard.
3. Build Platform Admin UI or provide a documented, access-controlled operational API workflow.
4. Add unique index on `Application.ClientId`.
5. Complete missing test coverage for the refresh token expiry and revocation paths.

With P0 and P1 items complete and tested, the feature would be reclassifiable as **Production-ready with minor gaps**.
