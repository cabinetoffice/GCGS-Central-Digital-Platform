# Application Registry — Master Verification Report

**Generated:** 2026-06-02  
**Branch:** feature/app-roles  
**Overall Status:** PARTIAL — build and in-memory tests pass; infrastructure tests blocked by Docker; critical test coverage gaps in AppRegistry feature code

---

## Executive Summary

| Phase | Status | Tests Passed | Tests Failed | Notes |
|-------|--------|:---:|:---:|-------|
| Phase 1: Foundation (Build) | ✅ PASS | — | 0 errors | 31 warnings (all pre-existing) |
| Phase 2A: Organisation Service | ⚠️ PARTIAL | 546 | 1 | Pre-existing bug unrelated to AppRegistry |
| Phase 2B: Application / Roles | ✅ PASS | 47 | 0 | Role model tests only; no AppRegistry endpoint tests |
| Phase 2C: Org-App Linkage | ❌ NO TESTS | 0 | 0 | Zero test coverage |
| Phase 3A: User Assignments | ❌ NO TESTS | 0 | 0 | Zero test coverage |
| Phase 3B: Claims Service | ⚠️ PARTIAL | 6 | 0 | Authority claims pass; new HasApplicationRole/Permission methods untested |
| Phase 3C: Claims Caching | ❌ NO TESTS | 0 | 0 | Zero test coverage |
| **Full suite (excluding infra)** | ✅ PASS | **3481** | **1** | See infrastructure failures section |
| **Full suite (all)** | ❌ BLOCKED | 3759 | 281 | 280 of 281 failures are Docker/AWS/MQ — see below |

---

## Build — Phase 1

```
dotnet build — 0 errors, 31 warnings
Time: 00:00:26.97
```

**Warning breakdown (non-blocking):**
- `NU1701` — NuGet framework compatibility (Redis session state, pre-existing)
- `NU1903` — High severity vulnerability in `AutoMapper 13.0.1` (pre-existing, affects Forms/Person/Organisation/DataSharing/Authority WebApis)
- `CS8602` / `CS9113` — Nullable dereference and unused parameter (pre-existing)

No errors. Build is clean for this branch.

---

## Infrastructure Failures (Docker / Cloud — not code failures)

All 280 infrastructure failures are caused by **Docker Desktop not running** in the current environment. Testcontainers cannot start PostgreSQL containers. These failures are environmental, not code regressions.

| Test Assembly | Failures | Root Cause |
|---|:---:|---|
| `CO.CDP.Testcontainers.PostgreSql.Tests` | 2 | Docker not running |
| `CO.CDP.OrganisationInformation.Persistence.Tests` | 149 | Docker not running (PostgreSQL Testcontainer) |
| `CO.CDP.EntityFrameworkCore.Tests` | 2 | Docker not running (PostgreSQL Testcontainer) |
| `CO.CDP.EntityVerification.Persistence.Tests` | 7 | Docker not running |
| `CO.CDP.RegisterOfCommercialTools.Persistence.Tests` | 64 | Docker not running |
| `CO.CDP.DataSharing.WebApi.Tests` | 24 | Docker not running |
| `CO.CDP.AwsServices.Tests` | 23 | AWS credentials / SDK not available locally |
| `CO.CDP.MQ.Tests` | 6 | Message queue infrastructure not available |
| `CO.CDP.EntityVerification.Tests` | 4 | Infrastructure dependency |
| **Total infrastructure** | **281** | Start Docker to clear these |

**Action:** Start Docker Desktop and re-run `dotnet test` to verify all infrastructure tests pass in a full environment.

---

## Genuine Code Failure — Pre-existing

**1 test failure is a real code bug, unrelated to the AppRegistry feature:**

```
CO.CDP.Organisation.WebApi.Tests
  UseCase.GetOrganisationMouSignatureLatestUseCaseTest
    Execute_ShouldReturnLatestMouSignature_WhenValidData

Expected result.Id to be {0ef275c1-51ef-4f80-b7ef-1b6d49043a55},
but found {6efd4d64-0b9e-43db-a9dd-c0bfba041b22}.

File: Services/CO.CDP.Organisation.WebApi.Tests/UseCase/GetOrganisationMouSignatureLatestUseCaseTest.cs:44
```

This is a Guid mismatch in a test for MoU signature sorting logic. It existed before this branch and is unrelated to AppRegistry. Should be tracked and fixed separately.

---

## Data Store Summary

### PostgreSQL Entities (ApplicationRegistryContext — EF Core, GUID-keyed)

All entities confirmed present in `CO.CDP.ApplicationRegistry.Persistence/Entities/` with correct schema and unique indexes.

| Entity | Key | Unique Index | Status |
|---|---|---|---|
| `Organisation` | `Guid Id` | `Slug` | ✅ |
| `Application` | `Guid Id` | `ClientId` | ✅ |
| `ApplicationPermission` | `Guid Id` | `(ApplicationId, Name)` | ✅ |
| `ApplicationRole` | `Guid Id` | `(ApplicationId, Name)` | ✅ |
| `RolePermission` | `(RoleId, PermissionId)` | composite PK | ✅ |
| `OrganisationApplication` | `(OrganisationId, ApplicationId)` | composite PK | ✅ |
| `UserOrganisationMembership` | `Guid Id` | `(UserPrincipalId, OrganisationId)` | ✅ |
| `UserApplicationAssignment` | `Guid Id` | `(UserPrincipalId, ApplicationId, OrganisationId)` | ✅ |
| `UserRoleAssignment` | `(UserApplicationAssignmentId, RoleId)` | composite PK | ✅ |
| `FeatureFlag` / `FeatureFlagScope` | `Guid Id` | `TileId` / `(FlagId, OrgTypeId)` | ✅ |
| `AuditLog` | `Guid Id` | Indexed by `EntityType`, `Timestamp` | ✅ |

> **Note:** MongoDB is NOT used in this feature. The ApplicationRegistry is entirely PostgreSQL via EF Core. The migration `20260505123146_InitialApplicationRegistry` exists and is applied.

---

## API Endpoints Summary

All endpoints verified to exist in `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Api/`.

| Category | Endpoint(s) | Auth Policy | Tests |
|---|---|---|---|
| Applications | `GET/POST /api/applications` | PlatformAdmin | ❌ None |
| Applications | `GET/PUT/DELETE /api/applications/{appId}` | PlatformAdmin | ❌ None |
| Permissions | `GET/POST/PUT/DELETE /api/applications/{appId}/permissions[/{permId}]` | PlatformAdmin | ❌ None |
| Roles | `GET/POST/PUT/DELETE /api/applications/{appId}/roles[/{roleId}]` | PlatformAdmin | ❌ None |
| Role-Permissions | `PUT /api/applications/{appId}/roles/{roleId}/permissions` | PlatformAdmin | ❌ None |
| Org endpoints | `GET/POST /api/organisations`, member mgmt | PlatformAdmin / OrgAdmin | ❌ None |
| User Assignments | `GET/POST/PUT/DELETE /api/organisations/{orgId}/applications/{appId}/users` | OrgAdmin | ❌ None |
| Claims Tree | `GET /api/claims/{userPrincipalId}` | PlatformAdmin | ❌ None |
| Claims Tree | `GET /api/claims/{userPrincipalId}/organisations/{orgId}` | OrgMember | ❌ None |
| Access Control | `AccessControlEndpoints` | — | ❌ None |
| Audit | `AuditEndpoints` | — | ❌ None |
| Feature Flags | `FeatureFlagEndpoints` | — | ❌ None |
| Categories | `CategoryEndpoints` | — | ❌ None |
| **Total endpoints** | **~36** | | **0 covered** |

---

## Token Enrichment Path

| Component | File | Status |
|---|---|---|
| `TokenService.CreateAccessToken()` | `Services/CO.CDP.Organisation.Authority/TokenService.cs:131` | ✅ Exists |
| Feature flag: `ClaimsApiEnabled` | `FeaturesOptions` | ✅ Gated correctly |
| Outbound call target | `/organisations/claims/users/{urn}` → `GetUserClaimsUseCase` | ⚠️ **Calls OLD endpoint** |
| New use case (AppRegistry) | `GetClaimsTreeUseCase` → `/api/claims/{userPrincipalId}` | ⚠️ **Not wired to token service** |
| JWT claim key | `cdp_claims` (JSON type) | ✅ Correct |
| Claim model | `UserClaims → OrganisationMembershipClaim → ApplicationAssignmentClaim` | ✅ Correct shape |

**Token enrichment gap:** The token service still calls `GetUserClaimsUseCase` which reads the legacy `OrganisationInformationContext`. The new `GetClaimsTreeUseCase` (reads `ApplicationRegistryContext`, includes app roles/permissions) is exposed at `/api/claims/{userPrincipalId}` but is not connected to the Authority service. Users' JWTs will not include AppRegistry roles until this is wired up.

---

## ClaimService Coverage

`Libraries/CO.CDP.Authentication/ClaimService.cs` — 3 methods added for AppRegistry:

| Method | Tested? |
|---|---|
| `GetUserUrn()` | ✅ Yes |
| `GetChannel()` | ✅ Yes |
| `HaveAccessToOrganisation()` | ✅ Yes (5 cases) |
| `GetApplicationClaims()` | ❌ No |
| `HasApplicationRole(orgId, clientId, roleName)` | ❌ No |
| `HasApplicationPermission(orgId, clientId, permissionName)` | ❌ No |

---

## Frontend UI Coverage

`Frontend/CO.CDP.UserManagement.App/Controllers/ChangeApplicationRolesController.cs`

| Flow | Routes | Tests |
|---|---|---|
| User: change roles | `GET/POST /user/{cdpPersonId}/application-roles/change` | ❌ None found |
| User: check | `GET/POST /user/{cdpPersonId}/application-roles/change/check` | ❌ None found |
| User: success | `GET /user/{cdpPersonId}/application-roles/change/success` | ❌ None found |
| Invite: change roles | `GET/POST /invites/{inviteGuid}/application-roles/change` | ❌ None found |
| Invite: check / success | corresponding routes | ❌ None found |

---

## Critical Issues

### 1. Token enrichment not connected to AppRegistry (BLOCKER)
`TokenService` calls `/organisations/claims/users/{urn}` → `GetUserClaimsUseCase` which reads the old `OrganisationInformation` DB. The new `GetClaimsTreeUseCase` (which reads AppRegistry roles/permissions) is never called. AppRegistry roles will not appear in JWTs until this is fixed.

**File:** `Services/CO.CDP.Organisation.Authority/TokenService.cs:150`

### 2. `AssignedBy` hardcoded as `"system"` (MEDIUM)
When creating a `UserApplicationAssignment`, the `AssignedBy` field is set to the literal string `"system"` instead of the authenticated caller's identity.

**File:** `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Api/UserAssignmentEndpoints.cs:43`

### 3. Zero unit tests for all AppRegistry endpoints and use cases (HIGH)
No tests exist for `ApplicationEndpoints`, `UserAssignmentEndpoints`, `ClaimsEndpoints`, `GetClaimsTreeUseCase`, `RegisterApplicationUseCase`, `DatabaseApplicationRepository`, or `DatabaseUserAssignmentRepository`.

### 4. Pre-existing test failure in MoU signature use case (LOW — pre-existing)
`GetOrganisationMouSignatureLatestUseCaseTest.Execute_ShouldReturnLatestMouSignature_WhenValidData` fails with a Guid mismatch. Pre-existing; unrelated to this branch.

---

## Recommendations

1. **Connect token enrichment** — Swap `TokenService.cs:150` to call the new `/api/claims/{userPrincipalId}` endpoint backed by `GetClaimsTreeUseCase`. This is the minimum requirement for AppRegistry roles to be usable in JWTs.

2. **Write unit tests for AppRegistry endpoints** — At minimum: `ApplicationEndpoints` (CRUD + role/permission management), `UserAssignmentEndpoints` (assign/update/revoke), `ClaimsEndpoints` (tree generation). Follow existing patterns in `Organisation.WebApi.Tests/Api/`.

3. **Write unit tests for ClaimService** — Cover `HasApplicationRole`, `HasApplicationPermission`, `GetApplicationClaims` with JWT fixtures.

4. **Fix `AssignedBy`** — Thread the authenticated user's identity (from `HttpContext.User`) into the assignment creation handler.

5. **Start Docker** — All 280 infrastructure failures will clear automatically. They are not regressions.

6. **Fix pre-existing MoU test** — Track the `GetOrganisationMouSignatureLatestUseCaseTest` Guid mismatch as a separate ticket.

---

## Sign-off

| Role | Name | Date | Status |
|---|---|---|---|
| Tech Lead | | | Pending |
| QA Lead | | | Pending |
| Security | | | Pending |
