# Application Registry — Master Verification Report

**Branch:** feature/app-roles
**Date:** 2026-06-03
**Verified by:** Automated discovery + phase verification passes

---

## Executive Summary

| Phase | Description | Status | Critical Issues |
|---|---|---|---|
| Phase 1 | Foundation (Build, Entities, Repos, Auth, MongoDB) | PARTIAL | 3 |
| Phase 2A | Organisation Service | PARTIAL | 4 |
| Phase 2B | Application Service & MongoDB (Roles, Permissions) | PARTIAL | 6 |
| Phase 2C | Organisation-Application Linkage | PARTIAL | 6 |
| Phase 3A | User Assignments (MongoDB) | PARTIAL | 4 |
| Phase 3B | Claims Service & Token Enrichment | PARTIAL | 4 |
| Phase 3C | Claims Caching | FAIL | 6 |
| **Overall** | | **PARTIAL** | **33** |

---

## Build & Test Health

| Metric | Result |
|---|---|
| Build errors | ✅ 0 |
| Build warnings | ⚠️ 31 |
| Unit / integration tests passed | ✅ 89 |
| Unit / integration tests failed | ✅ 0 |
| E2E tests run | Not verified in this pass |

Test breakdown: `Organisation.WebApi.Tests` ApplicationRegistry filter — 69 passed. `Authentication.Tests` ClaimService filter — 20 passed.

---

## Most Significant Architectural Deviation

**MongoDB-Only vs. Spec PostgreSQL + MongoDB Hybrid**

The spec describes a hybrid where only Roles, Permissions, and UserRoleAssignments are stored in MongoDB, with Applications, Organisations, Members, and UserApplicationAssignments in PostgreSQL. The actual implementation uses **MongoDB exclusively** for all AppRegistry entities. This is an intentional decision noted in `Program.cs` architecture comments. The existing `OrganisationInformation` entities continue to use PostgreSQL (EF Core / Npgsql); only the AppRegistry sub-system is MongoDB-backed.

This deviation has cascading effects across phases:

- **Phase 2B:** The spec's `(applicationId, name)` compound index on roles and permissions is structurally impossible because those entities are embedded arrays, not separate collections.
- **Phase 3A:** `UserApplicationAssignment` is in MongoDB, not PostgreSQL as the spec requires.
- **Phase 3C:** The caching layer designed for the hybrid was not ported to CO-CDP.

---

## Phase 1 — Foundation

**Status: PARTIAL**

All 16 entities, 7 repository interfaces, 8 MongoDB collections, and 7 indexes are present. DI wiring is complete. The build succeeds with 0 errors and 31 warnings.

| Finding | Severity |
|---|---|
| ServiceAccount authorization policy is absent | High |
| GlobalExceptionHandler is dead code — registered nowhere in Program.cs | Low |
| Policy names OrgAdmin/OrgMember deviate from spec names OrganisationAdmin/OrganisationMember | Medium |

Full details: [phase1-foundation-report.md](phase1-foundation-report.md)

---

## Phase 2A — Organisation Service

**Status: PARTIAL**

All 6 spec endpoints are present. All 6 organisation use cases are implemented. Slug generation works on create. The initial discovery pass underreported the endpoint surface (listed 2 routes; actual file implements 13 routes).

| Finding | Severity |
|---|---|
| UpdateOrganisationUseCase does not check for slug collision on rename — risks duplicate key exception | High |
| AddMemberUseCase cannot reactivate soft-deleted members | Medium |
| DELETE member bypasses use-case layer — calls repo directly | Medium |
| Enable/disable application endpoints are stubs with no persistence | High |

Full details: [phase2a-organisation-service-report.md](phase2a-organisation-service-report.md)

---

## Phase 2B — Application Service & MongoDB

**Status: PARTIAL**

Application CRUD, role, and permission endpoints are present. Storage is MongoDB embedded arrays (not a separate collection per spec). Role and permission mutations bypass the use-case layer.

| Finding | Severity |
|---|---|
| GET /api/applications/{appId}, GET /roles, GET /permissions have no authorization | High |
| Spec (applicationId, name) compound index impossible — entities are embedded arrays | Medium |
| Role and permission mutations bypass use-case layer | Medium |
| 7 role/permission use cases missing entirely | Medium |
| Raw BSON string interpolation in UpdatePermissionAsync | Medium |
| SetRolePermissionsAsync uses full document replace — lost-update risk | High |

Full details: [phase2b-application-service-mongodb-report.md](phase2b-application-service-mongodb-report.md)

---

## Phase 2C — Organisation-Application Linkage

**Status: PARTIAL**

The enable/disable endpoints exist but are stubs that return HTTP 201/204 with no persistence. `EnableApplicationAsync` and `DisableApplicationAsync` are implemented in the repository but are unreachable dead code at the API layer. Cascade disable to user assignments is not implemented.

| Finding | Severity |
|---|---|
| POST enable endpoint is a stub — no persistence | Critical |
| DELETE disable endpoint is a stub — no persistence | Critical |
| EnableApplicationAsync / DisableApplicationAsync are dead code | High |
| Cascade disable to UserApplicationAssignments not implemented | High |
| POST route path deviates from spec (appId in path vs. body) | Medium |
| No tests for enable/disable paths | Medium |

Full details: [phase2c-org-app-linkage-report.md](phase2c-org-app-linkage-report.md)

---

## Phase 3A — User Assignments (MongoDB)

**Status: PARTIAL**

Assignment list, assign, and revoke endpoints are present. The PUT update-roles endpoint exists but is missing the `/roles` suffix required by the spec. `AssignedBy` is hardcoded to `"system"` on all write operations.

| Finding | Severity |
|---|---|
| PUT update-roles endpoint missing /roles suffix — 404 for spec-compliant clients | High |
| AssignedBy hardcoded to "system" — real actor not recorded | High |
| UserApplicationAssignment in MongoDB, not PostgreSQL as spec requires | Medium (intentional) |
| No bulk RevokeAllAssignmentsAsync — blocks Phase 2C cascade requirement | High |

Full details: [phase3a-user-assignments-mongodb-report.md](phase3a-user-assignments-mongodb-report.md)

---

## Phase 3B — Claims Service & Token Enrichment

**Status: PARTIAL**

Both claims endpoints are present and `GetClaimsTreeUseCase` is wired to them. However, `TokenService` never calls the AppRegistry claims endpoint, so `cdp_app_claims` does not exist in any JWT and AppRegistry RBAC data is never surfaced to consuming services.

| Finding | Severity |
|---|---|
| cdp_app_claims JWT claim does not exist — AppRegistry RBAC invisible to all consumers | Critical |
| GetClaimsTreeUseCase never called during token issuance | Critical |
| GET /api/claims/{upn}/organisations/{orgId} uses OrgMember policy, spec requires PlatformAdmin | High |
| No ServiceAccount auth channel on /api/claims/* endpoints | High |

Full details: [phase3b-claims-service-report.md](phase3b-claims-service-report.md)

---

## Phase 3C — Claims Caching

**Status: FAIL**

Claims caching has not been ported from the `inventur-GCGS-Central-Digital-Platform` sibling repo into CO-CDP. `IClaimsCacheService`, `IClaimsService`, and `CachedClaimsService` are all absent. Redis is not registered. DI wiring is missing. Handlers and use cases that reference `IClaimsCacheService` hold an unresolvable dependency.

| Finding | Severity |
|---|---|
| IClaimsCacheService missing from CO-CDP | Critical |
| IClaimsService missing from CO-CDP | Critical |
| CachedClaimsService decorator missing from CO-CDP | Critical |
| Redis IDistributedCache not registered in Program.cs | Critical |
| AddUserManagementCaching() DI extension missing | Critical |
| No TTL configuration | High |

Full details: [phase3c-claims-caching-report.md](phase3c-claims-caching-report.md)

---

## Consolidated Issue Register

| ID | Phase | Description | Severity |
|---|---|---|---|
| ISSUE-1-01 | 1 | ServiceAccount authorization policy absent | High |
| ISSUE-1-02 | 1 | GlobalExceptionHandler is dead code | Low |
| ISSUE-1-03 | 1 | Policy naming mismatch (OrgAdmin vs OrganisationAdmin) | Medium |
| ISSUE-2A-01 | 2A | UpdateOrganisationUseCase missing slug collision check on rename | High |
| ISSUE-2A-02 | 2A | AddMemberUseCase cannot reactivate soft-deleted members | Medium |
| ISSUE-2A-03 | 2A | DELETE member bypasses use-case layer | Medium |
| ISSUE-2A-04 | 2A | Enable/disable application endpoints are stubs | High |
| ISSUE-2B-01 | 2B | Three unauthenticated GET endpoints on Application sub-resources | High |
| ISSUE-2B-02 | 2B | Spec compound index impossible — embedded array storage | Medium |
| ISSUE-2B-03 | 2B | Role/permission mutations bypass use-case layer | Medium |
| ISSUE-2B-04 | 2B | 7 role/permission use cases entirely missing | Medium |
| ISSUE-2B-05 | 2B | Raw BSON string interpolation in UpdatePermissionAsync | Medium |
| ISSUE-2B-06 | 2B | SetRolePermissionsAsync uses full document replace — lost-update risk | High |
| ISSUE-2C-01 | 2C | POST enable endpoint is a stub — no persistence | Critical |
| ISSUE-2C-02 | 2C | DELETE disable endpoint is a stub — no persistence | Critical |
| ISSUE-2C-03 | 2C | EnableApplicationAsync / DisableApplicationAsync are dead code | High |
| ISSUE-2C-04 | 2C | Cascade disable to UserApplicationAssignments not implemented | High |
| ISSUE-2C-05 | 2C | POST route path deviates from spec | Medium |
| ISSUE-2C-06 | 2C | No tests for enable/disable paths | Medium |
| ISSUE-3A-01 | 3A | PUT update-roles endpoint missing /roles suffix | High |
| ISSUE-3A-02 | 3A | AssignedBy hardcoded to "system" on all write operations | High |
| ISSUE-3A-03 | 3A | UserApplicationAssignment in MongoDB, not PostgreSQL | Medium |
| ISSUE-3A-04 | 3A | No bulk RevokeAllAssignmentsAsync — blocks Phase 2C cascade | High |
| ISSUE-3B-01 | 3B | cdp_app_claims JWT claim does not exist | Critical |
| ISSUE-3B-02 | 3B | Wrong auth policy on second claims endpoint | High |
| ISSUE-3B-03 | 3B | No ServiceAccount auth channel on /api/claims/* | High |
| ISSUE-3B-04 | 3B | GetClaimsTreeUseCase never called during token issuance | Critical |
| ISSUE-3C-01 | 3C | IClaimsCacheService missing from CO-CDP | Critical |
| ISSUE-3C-02 | 3C | CachedClaimsService missing from CO-CDP | Critical |
| ISSUE-3C-03 | 3C | Redis not registered in Program.cs | Critical |
| ISSUE-3C-04 | 3C | No TTL configuration | High |
| ISSUE-3C-05 | 3C | AddUserManagementCaching() DI extension absent | Critical |
| ISSUE-3C-06 | 3C | IClaimsService missing from CO-CDP | Critical |

---

## Priority Remediation Order

The following ordering is recommended based on blocking dependencies:

**P0 — Must fix before feature branch can go to review:**

1. ISSUE-2C-01 / ISSUE-2C-02: Implement POST enable and DELETE disable handlers (currently stubs — feature is non-functional).
2. ISSUE-3B-01 / ISSUE-3B-04: Wire `GetClaimsTreeUseCase` into `TokenService` and emit `cdp_app_claims` — without this the entire AppRegistry RBAC model has no JWT representation.
3. ISSUE-3C-01 to ISSUE-3C-06: Port caching layer from inventur repo into CO-CDP.

**P1 — Fix before merge to main:**

4. ISSUE-2B-01: Add authorization to the three unauthenticated GET endpoints.
5. ISSUE-3A-01: Fix PUT route path (missing /roles suffix).
6. ISSUE-3A-02: Populate AssignedBy from authenticated caller, not hardcoded "system".
7. ISSUE-3A-04: Add RevokeAllAssignmentsAsync to unblock cascade disable.
8. ISSUE-2C-04: Implement cascade disable to user assignments.
9. ISSUE-1-01: Add ServiceAccount policy.
10. ISSUE-3B-02 / ISSUE-3B-03: Fix second claims endpoint policy and add ServiceAccount channel.

**P2 — Quality / architecture improvements:**

11. ISSUE-2A-01: Slug collision check on rename.
12. ISSUE-2B-06: Replace ReplaceOneAsync with targeted UpdateOneAsync in SetRolePermissionsAsync.
13. ISSUE-2B-05: Replace raw BSON string interpolation.
14. ISSUE-2B-03 / ISSUE-2B-04: Extract role/permission use cases.
15. ISSUE-2A-02 / ISSUE-2A-03: Member reactivation and use-case encapsulation.

---

## Files Verified

### Persistence Layer
- `Services/CO.CDP.ApplicationRegistry.Persistence/` — entities, repository interfaces, MongoDB implementations, BSON configuration

### API Layer
- `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Api/` — all endpoint files
- `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/UseCase/` — all use case files

### Frontend
- `Frontend/CO.CDP.OrganisationApp/Pages/Applications/` — 13 Razor pages (buyer-facing UI)

### Auth / Claims
- `Services/CO.CDP.Organisation.Authority/TokenService.cs`
- `Libraries/CO.CDP.Authentication/ClaimService.cs`

### Tests
- `Services/CO.CDP.Organisation.WebApi.Tests/Api/ApplicationRegistry/` — 69 tests
- `Libraries/CO.CDP.Authentication.Tests/ClaimServiceTests.cs` — 20 tests
- `E2ETests/Tests/Applications/` — functional and navigation tests (not run in this pass)
