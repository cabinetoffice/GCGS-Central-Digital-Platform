# Phase 3B — Claims Service Verification Report

**Branch:** feature/app-roles
**Date:** 2026-06-03
**Status:** PARTIAL

---

## Claims Endpoints

| Endpoint | Auth Policy | Found |
|---|---|---|
| GET /api/claims/{userPrincipalId} | PlatformAdmin | ✅ |
| GET /api/claims/{userPrincipalId}/organisations/{orgId:guid} | OrgMember | ⚠️ Found but wrong policy (spec requires PlatformAdmin) |

---

## GetClaimsTreeUseCase

| Check | Result |
|---|---|
| GetClaimsTreeUseCase exists | ✅ |
| Registered in DI | ✅ |
| Wired to /api/claims/* endpoints | ✅ |
| Called by TokenService during token issuance | ❌ Never called |

---

## Token Enrichment Gap

| Check | Result |
|---|---|
| TokenService calls /organisations/claims/users/{urn} (legacy) | ✅ |
| TokenService calls /api/claims/{userPrincipalId} (AppRegistry) | ❌ |
| cdp_claims JWT claim populated | ✅ (from legacy endpoint) |
| cdp_app_claims JWT claim populated | ❌ Does not exist anywhere in the codebase |
| AppRegistry data surfaced in any JWT | ❌ |

**Token enrichment gap:** `TokenService` (`/Services/CO.CDP.Organisation.Authority/TokenService.cs` line 150) calls the legacy `/organisations/claims/users/{urn}` endpoint (`GetUserClaimsUseCase`, EF/Postgres-backed) and embeds the result as `cdp_claims`. It does not call `/api/claims/{userPrincipalId}`. `GetClaimsTreeUseCase` (MongoDB-backed AppRegistry) is never invoked by `TokenService`. The `cdp_app_claims` claim does not exist anywhere in the codebase — token enrichment via the AppRegistry claims tree is entirely absent.

---

## ClaimService (Consumer Side)

| Method | Found |
|---|---|
| GetUserUrn() | ✅ |
| GetUserRoles() | ✅ |
| GetOrganisationId() | ✅ |
| HaveAccessToOrganisation(orgId, scopes, personScopes) | ✅ |
| GetChannel() | ✅ |
| GetApplicationClaims() — deserialises cdp_claims as UserClaims | ✅ |
| HasApplicationRole(orgId, clientId, roleName) | ✅ |
| HasApplicationPermission(orgId, clientId, permissionName) | ✅ |

`HasApplicationRole` and `HasApplicationPermission` are implemented and check AppReg role/permission via `cdp_claims`. However, because `cdp_app_claims` is never populated (see token enrichment gap above), these helpers currently check the legacy `cdp_claims` structure, not the new AppRegistry claims tree.

---

## ServiceAccount Auth on Claims Endpoints

| Check | Result |
|---|---|
| ServiceAccount channel permitted on GET /api/claims/{userPrincipalId} | ❌ |
| ServiceAccount channel permitted on GET /api/claims/{userPrincipalId}/organisations/{orgId} | ❌ |

The spec expects `TokenService` (acting as a service account) to be able to call the `/api/claims/*` endpoints during token issuance. The current `PlatformAdmin` and `OrgMember` policies do not include `AuthenticationChannel.ServiceAccount`, so `TokenService` cannot call these endpoints even if wired to do so.

---

## Issues Found

### ISSUE-3B-01: cdp_app_claims JWT Claim Does Not Exist
**Severity:** Critical
`TokenService` never calls `GetClaimsTreeUseCase` or `/api/claims/{userPrincipalId}`. The `cdp_app_claims` claim is absent from `ClaimType` constants and is never added to any JWT. The entire AppRegistry RBAC model (roles, permissions, assignments) is therefore invisible to any consuming service that reads JWT claims.

**File:** `Services/CO.CDP.Organisation.Authority/TokenService.cs` line 150

### ISSUE-3B-02: Wrong Authorization Policy on Second Claims Endpoint
**Severity:** High
`GET /api/claims/{userPrincipalId}/organisations/{orgId}` is protected by `OrgMember` policy. The spec requires `PlatformAdmin`. An `OrgMember` can therefore enumerate claims for any user principal within their organisation, which may expose data they should not see.

**File:** `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Api/ClaimsEndpoints.cs`

### ISSUE-3B-03: No ServiceAccount Auth Channel on Claims Endpoints
**Severity:** High
The `/api/claims/*` endpoints do not include `AuthenticationChannel.ServiceAccount` in their authorization policies. `TokenService` operates as a service account client. Without this channel permitted, `TokenService` cannot call the AppRegistry claims endpoint even after wiring is added (ISSUE-3B-01), because the auth handler will reject the request.

### ISSUE-3B-04: GetClaimsTreeUseCase Never Invoked During Token Issuance
**Severity:** Critical
`GetClaimsTreeUseCase` is registered in DI and wired to `/api/claims/*` endpoints but is never called during token issuance. The MongoDB-backed AppRegistry data is therefore never surfaced in any JWT. This is the known token-enrichment gap documented in project memory.

---

## Recommendations

1. Wire `GetClaimsTreeUseCase` to `TokenService`: when `ClaimsApiEnabled` is true, call `GET /api/claims/{encodedUrn}` (the new AppRegistry endpoint) and embed the JSON result as a second claim named `cdp_app_claims` in the JWT alongside the existing `cdp_claims`.
2. Add `AuthenticationChannel.ServiceAccount` to the authorization policy on both `/api/claims/*` endpoints so that `TokenService` (a service account client) can call them during token issuance.
3. Change `GET /api/claims/{userPrincipalId}/organisations/{orgId}` authorization policy from `OrgMember` to `PlatformAdmin` to match the spec, or introduce a dedicated `ServiceAccountOrPlatformAdmin` composite policy.
4. Add `ClaimType.CdpAppClaims` constant to `/Libraries/CO.CDP.Authentication/Constants.cs` and add a `GetApplicationRegistryClaims()` helper to `IClaimService` / `ClaimService` that deserialises the `cdp_app_claims` claim. `HasApplicationRole` and `HasApplicationPermission` should then optionally fall back to or prefer this richer source over the legacy `cdp_claims` structure.
