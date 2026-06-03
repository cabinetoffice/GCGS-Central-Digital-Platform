# Phase 2A — Organisation Service Verification Report

**Branch:** feature/app-roles
**Date:** 2026-06-03
**Status:** PARTIAL

---

## Endpoints

| Endpoint | Auth Policy | Found |
|---|---|---|
| GET /api/organisations | PlatformAdmin | ✅ |
| POST /api/organisations | PlatformAdmin | ✅ |
| GET /api/organisations/{orgId:guid} | PlatformAdmin | ✅ |
| PUT /api/organisations/{orgId:guid} | PlatformAdmin | ✅ |
| GET /api/organisations/{orgId:guid}/members | PlatformAdmin / OrgAdmin | ✅ |
| POST /api/organisations/{orgId:guid}/members | PlatformAdmin / OrgAdmin | ✅ |

All 6 spec endpoints are present. The initial discovery pass against `OrganisationEndpoints.cs` had underreported the surface — it listed only 2 routes (GET and POST `/api/organisations`). The actual file implements all 6 spec endpoints plus 7 additional routes including GET/PUT `/{orgId}`, member sub-routes, application sub-routes, and a slug route.

---

## Use Cases

| Use Case | Found |
|---|---|
| GetOrganisationUseCase | ✅ |
| GetOrganisationsUseCase | ✅ |
| RegisterOrganisationUseCase | ✅ |
| UpdateOrganisationUseCase | ✅ |
| GetMembersUseCase | ✅ |
| AddMemberUseCase | ✅ |

All 6 organisation use cases are implemented and registered in DI.

---

## Repository Methods

| Method | Found |
|---|---|
| GetByIdAsync | ✅ |
| GetBySlugAsync | ✅ |
| GetAllAsync(name, type) | ✅ |
| CreateAsync | ✅ |
| UpdateAsync | ✅ |
| GetMembersAsync | ✅ |
| GetMemberAsync | ✅ |
| AddMemberAsync | ✅ |
| UpdateMemberAsync | ✅ |
| GetOrganisationApplicationsAsync | ✅ |
| EnableApplicationAsync | ✅ (implemented but never called — see Phase 2C) |
| DisableApplicationAsync | ✅ (implemented but never called — see Phase 2C) |

---

## Slug Generation

| Check | Result |
|---|---|
| SlugGenerator implemented | ✅ |
| Slug generated on create | ✅ |
| Slug collision detection on create | ✅ (appends Guid suffix on conflict) |
| Slug regenerated on rename (update) | ✅ |
| Slug collision detection on update | ❌ (missing — see ISSUE-2A-01) |

---

## Member Management

| Check | Result |
|---|---|
| Add member implemented | ✅ |
| Get members implemented | ✅ |
| Duplicate member guard | ⚠️ (throws, but does not handle reactivation of soft-deleted members — see ISSUE-2A-02) |
| Remove member endpoint exists | ✅ |
| Remove member via use case | ❌ (calls repo directly, bypassing use-case layer — see ISSUE-2A-03) |

---

## Issues Found

### ISSUE-2A-01: UpdateOrganisationUseCase Missing Slug Collision Check
**Severity:** High
`UpdateOrganisationUseCase` regenerates the slug on rename (via `SlugGenerator.Generate`) but does NOT call `GetBySlugAsync` to check for a collision, unlike `RegisterOrganisationUseCase` which appends a Guid suffix when a slug conflict is found. A rename to a name whose slug already exists will silently produce a duplicate slug and violate the unique index `idx_organisation_slug`, causing an unhandled MongoDB duplicate key exception at the persistence layer.

**File:** `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/UseCase/Organisation/UpdateOrganisationUseCase.cs` line 24

### ISSUE-2A-02: AddMemberUseCase Cannot Reactivate Soft-Deleted Members
**Severity:** Medium
`AddMemberUseCase` throws `InvalidOperationException` ("User is already a member") when `GetMemberAsync` returns a result. However, `GetMemberAsync` only returns active members. A previously soft-deleted member cannot be re-added; the correct behaviour would be to detect an inactive existing membership and reactivate it rather than throwing.

**File:** `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/UseCase/Organisation/AddMemberUseCase.cs` lines 22–24

### ISSUE-2A-03: DELETE Member Bypasses Use-Case Layer
**Severity:** Medium
`DELETE /api/organisations/{orgId}/members/{userId}` calls `IOrganisationRepository` directly from the endpoint handler, bypassing the use-case layer. The deactivation path therefore has no use-case encapsulation and skips any future business-logic hooks (e.g. audit logging, cascade revocation).

**File:** `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Api/OrganisationEndpoints.cs` lines 110–125

### ISSUE-2A-04: Enable/Disable Application Endpoints Are Stubs
**Severity:** High
`POST /api/organisations/{orgId}/applications/{appId:guid}` and `DELETE /api/organisations/{orgId}/applications/{appId:guid}` return `Created` and `NoContent` respectively with no business logic. `EnableApplicationAsync` and `DisableApplicationAsync` are never called from these handlers. This is covered in detail in the Phase 2C report.

**File:** `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Api/OrganisationEndpoints.cs` lines 146–162

---

## Recommendations

1. Fix `UpdateOrganisationUseCase` to call `GetBySlugAsync` after regenerating the slug on rename and append a short Guid suffix on collision, mirroring the pattern already in `RegisterOrganisationUseCase`.
2. Extend `AddMemberUseCase` to detect an inactive existing membership (query without `IsActive` filter or add a `GetMemberIncludingInactiveAsync` method) and reactivate it instead of throwing, to support re-adding removed members.
3. Extract the DELETE-member logic from `OrganisationEndpoints` into a dedicated `RemoveMemberUseCase` (or extend `AddMemberUseCase` to handle deactivation) so the operation goes through the use-case layer and benefits from audit logging.
4. Implement `EnableApplicationAsync` and `DisableApplicationAsync` calls in the stub `POST`/`DELETE /api/organisations/{orgId}/applications/{appId:guid}` handlers, or return `501 Not Implemented` until Phase 2C covers them.
5. Re-run the discovery pass against `OrganisationEndpoints.cs` to capture all routes; the partial listing caused the discovery findings to underreport the endpoint surface.
