# Phase 3A — User Assignments (MongoDB) Verification Report

**Branch:** feature/app-roles
**Date:** 2026-06-03
**Status:** PARTIAL

---

## Architectural Deviation — Storage

**Critical:** The spec expects `UserApplicationAssignment` in PostgreSQL and `UserRoleAssignment` in MongoDB. The actual implementation stores both in the single MongoDB collection `app_registry_user_assignments`. This is an intentional architectural decision documented in the codebase but represents a confirmed divergence from the written spec.

---

## Endpoints

| Endpoint | Auth Policy | Found |
|---|---|---|
| GET /api/organisations/{orgId:guid}/applications/{appId:guid}/users | OrgAdmin | ✅ |
| POST /api/organisations/{orgId:guid}/applications/{appId:guid}/users | OrgAdmin | ✅ |
| DELETE /api/organisations/{orgId:guid}/applications/{appId:guid}/users/{userId} | OrgAdmin | ✅ |
| PUT /api/organisations/{orgId:guid}/applications/{appId:guid}/users/{userId}/roles | OrgAdmin | ⚠️ Path mismatch — /roles suffix missing |

---

## Entities

| Entity | Found | Storage |
|---|---|---|
| UserApplicationAssignment | ✅ | MongoDB (spec: PostgreSQL) |
| UserRoleAssignment | ✅ | MongoDB (embedded in UserApplicationAssignment) |

---

## Repository Methods

| Method | Interface | Mongo Implementation |
|---|---|---|
| GetAssignmentsAsync(orgId, appId) | ✅ | ✅ |
| GetAssignmentAsync(orgId, appId, userPrincipalId) | ✅ | ✅ |
| CreateAssignmentAsync | ✅ | ✅ |
| UpdateAssignmentAsync | ✅ | ✅ |
| RevokeAssignmentAsync | ✅ | ✅ |
| RevokeAllAssignmentsAsync (bulk by org+app) | ❌ Missing | ❌ Missing |

---

## MongoDB Index

| Index | Present |
|---|---|
| idx_userassignment_unique (unique compound: UserPrincipalId + ApplicationId + OrganisationId) | ✅ |

---

## Role Hydration

Roles assigned to a user are stored as `roleId` references inside `UserRoleAssignment` subdocuments within the `UserApplicationAssignment` MongoDB document. At read time, role objects are hydrated by resolving these `roleId` values against the `app_registry_applications` document. There is no separate roles collection involved.

---

## Audit / AssignedBy

| Check | Result |
|---|---|
| AssignedBy populated from authenticated caller on POST | ❌ Hardcoded to literal `"system"` |
| AssignedBy populated from authenticated caller on PUT | ❌ Hardcoded to literal `"system"` |

---

## Issues Found

### ISSUE-3A-01: PUT Route Path Mismatch — Missing /roles Suffix
**Severity:** High
The spec requires `PUT /api/organisations/{orgId}/applications/{appId}/users/{userId}/roles` (with a `/roles` suffix). The implementation registers `PUT /api/organisations/{orgId:guid}/applications/{appId:guid}/users/{userId}` — the trailing `/roles` segment is absent. Any client or integration test targeting the spec path will receive a `404 Not Found`.

**File:** `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Api/UserAssignmentEndpoints.cs`

### ISSUE-3A-02: AssignedBy Hardcoded to "system"
**Severity:** High
`AssignedBy` is hardcoded to the string literal `"system"` in the POST endpoint rather than being populated from the authenticated user's principal claim. The audit trail therefore records no real actor for assignment creation.

**File:** `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Api/UserAssignmentEndpoints.cs` line 44

### ISSUE-3A-03: Storage Deviation — Both Entities in MongoDB
**Severity:** Medium (confirmed intentional)
The spec expects `UserApplicationAssignment` in PostgreSQL and `UserRoleAssignment` in MongoDB. The implementation places both in MongoDB collection `app_registry_user_assignments`. This is intentional but represents a confirmed divergence from the written spec. If other services (e.g. EF-backed query services) need to join against `UserApplicationAssignment`, this deviation will cause runtime failures.

### ISSUE-3A-04: No Bulk Revoke Method on IUserAssignmentRepository
**Severity:** High (blocks Phase 2C cascade)
`IUserAssignmentRepository` only exposes `RevokeAssignmentAsync` for a single user. There is no `RevokeAllAssignmentsAsync(Guid organisationId, Guid applicationId)` method, making the Phase 2C cascade-disable requirement impossible to satisfy without a new interface method.

---

## Recommendations

1. Align the PUT route with the spec by appending `/roles` to the path: `app.MapPut("/api/organisations/{orgId:guid}/applications/{appId:guid}/users/{userId}/roles", ...)`. Update any client code or integration tests that reference the current path.
2. Inject `IHttpContextAccessor` or resolve the caller's user principal from `HttpContext` inside each write endpoint (`POST`, `PUT`, `DELETE`) and pass the real user identity to `AssignedBy` and the audit `UserId` fields instead of hardcoding `"system"`.
3. If the spec's PostgreSQL requirement for `UserApplicationAssignment` is a hard constraint (e.g. for cross-service joins or EF migrations in other services), document the MongoDB-only deviation formally or raise it as a spec change request. If not a constraint, update the spec to reflect the MongoDB-only design.
4. Add `IUserAssignmentRepository.RevokeAllAssignmentsAsync(Guid organisationId, Guid applicationId)` and implement it in `MongoUserAssignmentRepository` using `UpdateMany` to bulk-set `IsActive = false`. This also unblocks the Phase 2C cascade-disable requirement.
5. Add an integration test that POSTs a user assignment and asserts `AssignedBy` is the authenticated caller's principal, not `"system"`, to prevent silent regression.
