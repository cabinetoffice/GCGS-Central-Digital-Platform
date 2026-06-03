# Phase 1 — Foundation Verification Report

**Branch:** feature/app-roles
**Date:** 2026-06-03
**Status:** PARTIAL

---

## Build Health

| Check | Result |
|---|---|
| Build errors | ✅ 0 errors |
| Build warnings | ⚠️ 31 warnings |
| Overall build | ✅ Succeeded |

---

## Entities

| Entity | Found |
|---|---|
| AccessControlEntry | ✅ |
| Application | ✅ |
| ApplicationPermission | ✅ |
| ApplicationRole | ✅ |
| AuditLog | ✅ |
| CategoryPermission | ✅ |
| FeatureFlag | ✅ |
| FeatureFlagScope | ✅ |
| Organisation | ✅ |
| OrganisationApplication | ✅ |
| ReportCategory | ✅ |
| ReportCategoryAssignment | ✅ |
| RolePermission | ✅ |
| UserApplicationAssignment | ✅ |
| UserOrganisationMembership | ✅ |
| UserRoleAssignment | ✅ |

All 16 POCO entities are present. None carry BSON attributes directly — all serialisation is centralised in `BsonConfiguration.cs` using convention packs scoped to the `CO.CDP.ApplicationRegistry` namespace.

---

## Repository Interfaces

| Interface | Found |
|---|---|
| IAccessControlRepository | ✅ |
| IApplicationRepository | ✅ |
| IAuditRepository | ✅ |
| ICategoryRepository | ✅ |
| IFeatureFlagRepository | ✅ |
| IOrganisationRepository | ✅ |
| IUserAssignmentRepository | ✅ |

All 7 interfaces are present with MongoDB-backed implementations in `CO.CDP.ApplicationRegistry.Persistence/Repositories/MongoDB/`.

---

## MongoDB Configuration

| Check | Result |
|---|---|
| MongoDB configured | ✅ |
| Collections registered | ✅ 8 collections |
| Indexes created | ✅ 7 indexes |
| BsonConfiguration registered at startup | ✅ |
| MongoAppRegistryDatabase registered as Singleton | ✅ |

### Collections

| Collection Name |
|---|
| app_registry_applications |
| app_registry_organisations |
| app_registry_user_assignments |
| app_registry_feature_flags |
| app_registry_audit_logs |
| app_registry_access_control |
| app_registry_report_categories |
| app_registry_report_category_assignments |

### Indexes

| Index | Uniqueness | Fields |
|---|---|---|
| idx_application_clientId | Unique | Applications.ClientId |
| idx_application_active_name | Non-unique | Applications.IsActive + Name |
| idx_organisation_slug | Unique | Organisations.Slug |
| idx_organisation_members_upn | Non-unique | Organisations.members.userPrincipalId |
| idx_userassignment_unique | Unique compound | UserPrincipalId + ApplicationId + OrganisationId |
| idx_featureflag_tileId | Unique | FeatureFlags.TileId |
| idx_auditlog_entitytype_ts | Non-unique | AuditLogs.EntityType + Timestamp desc |

---

## Authorization Policies

| Policy | Found | Notes |
|---|---|---|
| PlatformAdmin | ✅ | Requires claim: type=`platform_role`, value=`admin`. Implemented in `PlatformAdminHandler`. |
| OrgAdmin | ✅ | Requires `org:{orgId}:role` claim with value `Admin` or `Owner`. Also passes for `platform_role=admin`. Implemented in `OrganisationRoleHandler`. |
| OrgMember | ✅ | Requires `org:{orgId}:role` claim with value `Member`, `Admin`, or `Owner`. Also passes for `platform_role=admin`. Implemented in `OrganisationRoleHandler`. |
| ServiceAccount | ❌ | Not defined in `AuthorizationPolicies.cs` or anywhere in the AppRegistry authorization layer. |

**Policy naming deviation:** The spec names the policies `OrganisationAdmin` and `OrganisationMember`, but the implementation uses the shorter names `OrgAdmin` and `OrgMember`.

---

## Exception Handling

| Check | Result |
|---|---|
| GlobalExceptionHandler class exists | ✅ |
| GlobalExceptionHandler registered in Program.cs | ❌ |
| ProblemDetails returned on exception | ✅ (via ResponseMiddleware / UseErrorHandler) |

---

## DI Registrations

| Registration | Lifetime | Status |
|---|---|---|
| BsonConfiguration.Register() | — | ✅ Called at startup |
| IMongoDatabase | Singleton | ✅ |
| MongoAppRegistryDatabase | Singleton | ✅ |
| IOrganisationRepository -> MongoOrganisationRepository | Scoped | ✅ |
| IApplicationRepository -> MongoApplicationRepository | Scoped | ✅ |
| IUserAssignmentRepository -> MongoUserAssignmentRepository | Scoped | ✅ |
| IAuditRepository -> MongoAuditRepository | Scoped | ✅ |
| IFeatureFlagRepository -> MongoFeatureFlagRepository | Scoped | ✅ |
| ICategoryRepository -> MongoCategoryRepository | Scoped | ✅ |
| IAccessControlRepository -> MongoAccessControlRepository | Scoped | ✅ |
| AppRegistry use cases (15 total) | Scoped | ✅ |
| PlatformAdminHandler + OrganisationRoleHandler | — | ✅ via AddApplicationRegistryAuthorization() |

---

## Architectural Deviation — MongoDB-Only vs. Spec Hybrid

**This is the most significant deviation in the codebase.**

The spec describes a PostgreSQL + MongoDB hybrid where only Roles, Permissions, and UserRoleAssignments are stored in MongoDB. The actual implementation uses **MongoDB exclusively** for all AppRegistry entities, including Applications, Organisations, Members, Roles, Permissions, and Assignments. The existing OrganisationInformation entities continue to use PostgreSQL (EF Core / Npgsql), but the AppRegistry sub-system is fully MongoDB-backed.

This appears to be an intentional design decision noted in `Program.cs` architecture comments. It has cascading implications for Phase 2B (indexing strategies), Phase 3A (storage expectation), and Phase 3C (caching).

---

## Issues Found

### ISSUE-1-01: Missing ServiceAccount Authorization Policy
**Severity:** High
The `ServiceAccount` authorization policy is absent from `AuthorizationPolicies.cs`. The spec requires it. Only `PlatformAdmin`, `OrgAdmin`, and `OrgMember` are defined. The broader `CO.CDP.Authentication` layer has `ServiceAccount` channel support via the `OrganisationAuthorize` attribute, but no named `ServiceAccount` policy exists in the AppRegistry authorization layer.

### ISSUE-1-02: Dead GlobalExceptionHandler
**Severity:** Low
`GlobalExceptionHandler` (at `Services/CO.CDP.Organisation.WebApi/ApplicationRegistry/Middleware/GlobalExceptionHandler.cs`) implements `IExceptionHandler` and returns `ProblemDetails` correctly, but is never registered in `Program.cs`. There is no `AddExceptionHandler<GlobalExceptionHandler>()` call. The pipeline uses `ResponseMiddleware` via `app.UseErrorHandler(ErrorCodes.Exception4xxMap)` instead, which also returns `ProblemDetails`. The spec criterion is met functionally but the AppRegistry-specific handler is unreachable dead code.

### ISSUE-1-03: Authorization Policy Naming Mismatch
**Severity:** Medium
The spec lists `OrganisationAdmin` and `OrganisationMember` as required policy names, but the implementation uses `OrgAdmin` and `OrgMember`. Any consumer or documentation that references the spec-defined names will not resolve correctly.

---

## Recommendations

1. Register `GlobalExceptionHandler` with `services.AddExceptionHandler<GlobalExceptionHandler>()` in `Program.cs` and call `app.UseExceptionHandler()` before `UseErrorHandler`, or remove the dead class and document that `ResponseMiddleware` satisfies the ProblemDetails requirement.
2. Add a `ServiceAccount` policy to `AuthorizationPolicies.cs` — e.g. `options.AddPolicy(ServiceAccount, policy => policy.Requirements.Add(new ServiceAccountRequirement()))` — with a corresponding handler that checks the `api_key` authentication scheme or a `service_account` claim.
3. Reconcile policy naming: either rename `OrgAdmin`/`OrgMember` to `OrganisationAdmin`/`OrganisationMember` to match the spec, or update the spec to reflect the shorter names used in the implementation.
