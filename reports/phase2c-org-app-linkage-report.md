# Phase 2C — Organisation-Application Linkage Verification Report

**Branch:** feature/app-roles
**Date:** 2026-06-03
**Status:** PARTIAL

---

## Endpoints

| Endpoint | Auth Policy | Found | Implemented |
|---|---|---|---|
| POST /api/organisations/{orgId:guid}/applications/{appId:guid} | PlatformAdmin | ✅ | ❌ Stub only |
| GET /api/organisations/{orgId:guid}/applications | PlatformAdmin / OrgAdmin | ✅ | ✅ |
| DELETE /api/organisations/{orgId:guid}/applications/{appId:guid} | PlatformAdmin | ✅ | ❌ Stub only |

**Route contract deviation:** The spec requires `POST /api/organisations/{orgId}/applications` (with `appId` in the body). The implementation uses `/api/organisations/{orgId}/applications/{appId:guid}` (with `appId` as a path segment).

---

## Entity

| Check | Result |
|---|---|
| OrganisationApplication entity exists | ✅ |
| OrganisationId property | ✅ |
| ApplicationId property | ✅ |
| EnabledAt property | ✅ |
| EnabledBy property | ✅ |

---

## Repository Methods

| Method | Defined in Interface | Implemented in Mongo | Called from Endpoint |
|---|---|---|---|
| EnableApplicationAsync | ✅ | ✅ | ❌ Never called |
| DisableApplicationAsync | ✅ | ✅ | ❌ Never called |
| GetOrganisationApplicationsAsync | ✅ | ✅ | ✅ |

`EnableApplicationAsync` and `DisableApplicationAsync` are correctly implemented in `MongoOrganisationRepository` and declared in `IOrganisationRepository`, but are unreachable dead code at the API layer because the endpoint handlers are stubs.

---

## Enable / Disable Logic

| Check | Result |
|---|---|
| Enable application persisted on POST | ❌ Stub returns 201 with no persistence |
| Disable application persisted on DELETE | ❌ Stub returns 204 with no persistence |
| Cascade disable to UserApplicationAssignments | ❌ Not implemented |
| Bulk-revoke method on IUserAssignmentRepository | ❌ Missing — only single-record RevokeAssignmentAsync exists |

---

## Tests

| Test Scenario | Covered |
|---|---|
| Successful enable | ❌ |
| Duplicate enable guard (idempotency / 409) | ❌ |
| Disable of non-linked app (404) | ❌ |
| Cascade revocation of user assignments on disable | ❌ |
| GET organisation applications | ✅ (via AppRegistryOrganisationEndpointsTests) |

---

## Issues Found

### ISSUE-2C-01: POST Enable Endpoint Is a Stub
**Severity:** Critical
`POST /api/organisations/{orgId:guid}/applications/{appId:guid}` returns `Results.Created()` without calling `EnableApplicationAsync` or any repository method. No request body is accepted and no `OrganisationApplication` is persisted.

**File:** `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Api/OrganisationEndpoints.cs` lines 146–162

### ISSUE-2C-02: DELETE Disable Endpoint Is a Stub
**Severity:** Critical
`DELETE /api/organisations/{orgId:guid}/applications/{appId:guid}` returns `Results.NoContent()` without calling `DisableApplicationAsync`. No application linkage is actually disabled.

**File:** `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Api/OrganisationEndpoints.cs` lines 146–162

### ISSUE-2C-03: EnableApplicationAsync and DisableApplicationAsync Are Dead Code
**Severity:** High
Both repository methods are fully implemented in `MongoOrganisationRepository` but are never invoked from any endpoint or use case. They are unreachable at the API layer.

### ISSUE-2C-04: Cascade Disable Not Implemented
**Severity:** High
When an application is disabled for an organisation, the spec requires all `UserApplicationAssignment` records for that org+app combination to be revoked. `DisableApplicationAsync` in `MongoOrganisationRepository` only removes the `OrganisationApplication` embedded document; it does not touch user assignment records. `IUserAssignmentRepository` lacks a bulk-revoke-by-application method — only a single-record `RevokeAssignmentAsync` (by org+app+userId) exists — making a cascade impossible without a new interface method.

### ISSUE-2C-05: Route Contract Mismatch
**Severity:** Medium
The spec requires `POST /api/organisations/{orgId}/applications` with `appId` in the request body. The implementation registers the route as `POST /api/organisations/{orgId}/applications/{appId:guid}` with `appId` as a path segment. This breaks any client targeting the spec route.

### ISSUE-2C-06: No Tests for Enable/Disable Paths
**Severity:** Medium
`AppRegistryOrganisationEndpointsTests` covers `GET /members` and `POST /members` but not the application enable/disable sub-routes. There are no tests verifying the current stub behaviour or any future implementation.

---

## Recommendations

1. Implement the `POST /api/organisations/{orgId:guid}/applications` handler body: accept an `EnableApplicationRequest` (containing `ApplicationId` and `EnabledBy`), load the application to verify it exists and is active, construct an `OrganisationApplication`, call `repo.EnableApplicationAsync(...)`, and return `201 Created`.
2. Implement the `DELETE /api/organisations/{orgId:guid}/applications/{appId:guid}` handler body: call `repo.DisableApplicationAsync(orgId, appId)` and cascade-disable all user assignments for that org+app combination before returning `204 No Content`.
3. Add `IUserAssignmentRepository.RevokeAllAssignmentsAsync(Guid organisationId, Guid applicationId)` and implement it in `MongoUserAssignmentRepository` using `UpdateMany` with a filter on `organisationId + applicationId` to set `IsActive = false` in a single operation.
4. Wire the cascade in the DELETE handler: after `DisableApplicationAsync`, call `userAssignmentRepo.RevokeAllAssignmentsAsync(orgId, appId)`.
5. Decide whether the POST route should take `appId` in the body (per spec) or as a path segment (current). Align to the spec unless there is a deliberate reason to diverge, and update API documentation accordingly.
6. Add integration tests covering: successful enable, duplicate-enable idempotency or `409 Conflict`, disable of a non-linked app returning `404`, and cascade revocation of user assignments on disable.
