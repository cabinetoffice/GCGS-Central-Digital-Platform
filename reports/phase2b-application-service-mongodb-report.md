# Phase 2B — Application Service & MongoDB Verification Report

**Branch:** feature/app-roles
**Date:** 2026-06-03
**Status:** PARTIAL

---

## Architectural Deviation — MongoDB-Only vs. Spec Hybrid

**Critical:** The spec describes a PostgreSQL + MongoDB hybrid where only Roles, Permissions, and UserRoleAssignments live in MongoDB. The actual implementation places **all** AppRegistry entities — including Applications, Organisations, Members, Roles, Permissions, and Assignments — exclusively in MongoDB. This deviation is intentional and documented in `Program.cs` architecture comments, but has significant downstream effects on indexing strategy and storage expectations described in this report.

---

## Application CRUD Endpoints

| Endpoint | Auth Policy | Found |
|---|---|---|
| GET /api/applications | PlatformAdmin | ✅ |
| POST /api/applications | PlatformAdmin | ✅ |
| GET /api/applications/{appId:guid} | None | ⚠️ Exists but unauthenticated |
| PUT /api/applications/{appId:guid} | PlatformAdmin | ✅ |
| DELETE /api/applications/{appId:guid} | PlatformAdmin | ⚠️ Exists but soft-delete only (IsActive=false) |

---

## Role Endpoints

| Endpoint | Auth Policy | Found |
|---|---|---|
| GET /api/applications/{appId:guid}/roles | None | ⚠️ Exists but unauthenticated |
| POST /api/applications/{appId:guid}/roles | PlatformAdmin | ✅ |
| PUT /api/applications/{appId:guid}/roles/{roleId:guid} | PlatformAdmin | ✅ |
| DELETE /api/applications/{appId:guid}/roles/{roleId:guid} | PlatformAdmin | ⚠️ Exists but soft-delete only (IsActive=false) |
| PUT /api/applications/{appId:guid}/roles/{roleId:guid}/permissions | PlatformAdmin | ✅ |

---

## Permission Endpoints

| Endpoint | Auth Policy | Found |
|---|---|---|
| GET /api/applications/{appId:guid}/permissions | None | ⚠️ Exists but unauthenticated |
| POST /api/applications/{appId:guid}/permissions | PlatformAdmin | ✅ |
| PUT /api/applications/{appId:guid}/permissions/{permId:guid} | PlatformAdmin | ✅ |
| DELETE /api/applications/{appId:guid}/permissions/{permId:guid} | PlatformAdmin | ✅ |

---

## Use Cases

| Use Case | Found |
|---|---|
| GetApplicationUseCase | ✅ |
| GetApplicationsUseCase | ✅ |
| RegisterApplicationUseCase | ✅ |
| UpdateApplicationUseCase | ✅ |
| CreatePermissionUseCase | ✅ |
| GetRolesUseCase | ❌ Missing |
| CreateRoleUseCase | ❌ Missing |
| UpdateRoleUseCase | ❌ Missing |
| DeleteRoleUseCase | ❌ Missing |
| GetPermissionsUseCase | ❌ Missing |
| UpdatePermissionUseCase | ❌ Missing |
| DeletePermissionUseCase | ❌ Missing |

Role and permission mutation endpoints call `IApplicationRepository` directly rather than through a use-case abstraction, which is inconsistent with the established architecture.

---

## Storage Approach

| Aspect | Spec Expectation | Actual Implementation |
|---|---|---|
| Applications | PostgreSQL | ✅ MongoDB (intentional deviation) |
| Roles | MongoDB (embedded or collection) | ✅ Embedded array within Application document |
| Permissions | MongoDB | ✅ Embedded array within Application document |
| RolePermission join | MongoDB | ✅ Embedded within each role subdocument; hydrated in-memory from permissions array on read |
| Separate roles collection | — | ❌ None — roles are embedded |
| Separate permissions collection | — | ❌ None — permissions are embedded |

`SetRolePermissionsAsync` performs a full document replace (`ReplaceOneAsync`) rather than a targeted array update. `UpdatePermissionAsync` uses a raw BSON string-interpolated `arrayFilter` which bypasses BSON convention serialisation.

---

## MongoDB Indexes

| Index | Required by Spec | Present |
|---|---|---|
| idx_application_clientId (unique, ClientId) | ✅ | ✅ |
| idx_application_active_name (IsActive + Name) | ✅ | ✅ |
| (applicationId, name) compound on roles | ✅ | ❌ Impossible — roles are embedded arrays |
| (applicationId, name) compound on permissions | ✅ | ❌ Impossible — permissions are embedded arrays |

The spec's `(applicationId, name)` compound index requirement for roles and permissions cannot be satisfied while these entities are embedded subdocuments within the Application MongoDB document. A multikey index on embedded arrays is structurally different from a compound index on separate collections.

---

## Issues Found

### ISSUE-2B-01: Unauthenticated Read Endpoints
**Severity:** High
Three GET endpoints have no `.RequireAuthorization()` call and are effectively public:
- `GET /api/applications/{appId:guid}`
- `GET /api/applications/{appId}/roles`
- `GET /api/applications/{appId}/permissions`

### ISSUE-2B-02: Missing Compound Index on Roles and Permissions
**Severity:** Medium
The spec requires MongoDB indexes on `(applicationId, name)` for roles and permissions. These indexes cannot be created because roles and permissions are embedded arrays inside the Application document rather than separate collections. A standard compound index on those fields is structurally impossible without document restructuring or moving them to sibling collections.

### ISSUE-2B-03: Role and Permission Mutations Bypass Use-Case Layer
**Severity:** Medium
Role and permission create/update/delete endpoints call `IApplicationRepository` directly. This is inconsistent with the use-case architecture used for applications and organisations, and makes these paths harder to test in isolation or enrich with cross-cutting concerns (audit, validation).

### ISSUE-2B-04: Missing Role and Permission Use Cases
**Severity:** Medium
No `GetRolesUseCase`, `CreateRoleUseCase`, `UpdateRoleUseCase`, `DeleteRoleUseCase`, `GetPermissionsUseCase`, `UpdatePermissionUseCase`, or `DeletePermissionUseCase` classes exist. Seven use cases are absent from the spec requirement.

### ISSUE-2B-05: Raw BSON String Interpolation in UpdatePermissionAsync
**Severity:** Medium
`UpdatePermissionAsync` uses a raw `BsonDocument` with string-interpolated `arrayFilter` (`${{\"perm.id\": \"{permission.Id}\"}}`), which bypasses BSON convention serialisation and is fragile if the Guid format or field name convention changes.

**File:** `Services/CO.CDP.ApplicationRegistry.Persistence/Repositories/MongoDB/MongoApplicationRepository.cs`

### ISSUE-2B-06: SetRolePermissionsAsync Uses Full Document Replace
**Severity:** High
`SetRolePermissionsAsync` performs a full application document replace (`ReplaceOneAsync`) rather than a targeted array update. This risks overwriting concurrent writes to other fields on the document (lost-update race).

**File:** `Services/CO.CDP.ApplicationRegistry.Persistence/Repositories/MongoDB/MongoApplicationRepository.cs`

---

## Recommendations

1. Add `.RequireAuthorization(AuthorizationPolicies.PlatformAdmin)` to the three unauthenticated GET endpoints: `GET /api/applications/{appId}`, `GET /api/applications/{appId}/roles`, `GET /api/applications/{appId}/permissions`.
2. Replace the raw string-interpolated `BsonDocument` `arrayFilters` in `UpdatePermissionAsync` and `UpdateRoleAsync` with strongly-typed `BsonDocumentArrayFilterDefinition` or use a typed positional update to eliminate the Guid format fragility.
3. Replace the full document `ReplaceOneAsync` in `SetRolePermissionsAsync` with a targeted `UpdateOneAsync` using `arrayFilters` to set only the role's permissions subdocument, preventing lost-update races.
4. Extract role and permission mutations into dedicated use case classes (`CreateRoleUseCase`, `UpdateRoleUseCase`, `DeleteRoleUseCase`, `UpdatePermissionUseCase`, `DeletePermissionUseCase`) to match the established architecture and enable unit testing.
5. If the spec's `(applicationId, name)` index requirement is a hard constraint, consider moving roles and/or permissions to sibling collections (`app_registry_roles`, `app_registry_permissions`) so a standard compound index can be applied — alternatively document that the embedded approach makes this index unnecessary given full-document retrieval by `applicationId`.
