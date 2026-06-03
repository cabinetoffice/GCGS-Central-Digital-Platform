# Phase 3C — Claims Caching Verification Report

**Branch:** feature/app-roles
**Date:** 2026-06-03
**Status:** FAIL

---

## Summary

Phase 3C caching exists only in the sibling `inventur-GCGS-Central-Digital-Platform` repository and has not been ported to CO-CDP. `IClaimsCacheService`, `IClaimsService`, and `CachedClaimsService` are absent from all CO-CDP libraries and services. The `Microsoft.Extensions.Caching.StackExchangeRedis` NuGet package is referenced in `CO.CDP.Organisation.WebApi.csproj` but `AddStackExchangeRedisCache()` is never called in `Program.cs`. Handlers and use cases in CO-CDP reference `IClaimsCacheService` from `CO.CDP.UserManagement.Core.Interfaces`, but that interface is missing from the CO-CDP copy of the library, creating an unresolvable dependency at runtime.

---

## Caching Infrastructure

| Check | Result |
|---|---|
| IClaimsCacheService interface in CO-CDP | ❌ |
| IClaimsService interface in CO-CDP | ❌ |
| CachedClaimsService decorator in CO-CDP | ❌ |
| Redis IDistributedCache registered in Program.cs | ❌ |
| StackExchangeRedis NuGet package present | ✅ (referenced but unused) |
| TTL configured | ❌ |
| DI decorator registration (AddUserManagementCaching()) | ❌ |

---

## Cache Invalidation

| Check | Result |
|---|---|
| Cache invalidation calls present in handlers/use cases | ✅ (IClaimsCacheService.InvalidateCacheAsync() called) |
| IClaimsCacheService resolvable at runtime | ❌ Interface missing from CO-CDP |

Cache invalidation call sites exist (in `PersonScopesUpdatedHandler`, `PersonRemovedHandler`, `UpdateApplicationAssignmentUseCase`, etc.) but the interface they depend on (`CO.CDP.UserManagement.Core.Interfaces.IClaimsCacheService`) is not present in the CO-CDP copy of `UserManagement.Core`, making these calls unresolvable.

---

## Inventur Repo Comparison

| Component | inventur-GCGS-Central-Digital-Platform | CO-CDP |
|---|---|---|
| IClaimsCacheService | ✅ Libraries/CO.CDP.UserManagement.Core/Interfaces/ | ❌ Absent |
| IClaimsService | ✅ Libraries/CO.CDP.UserManagement.Core/Interfaces/ | ❌ Absent |
| CachedClaimsService | ✅ Services/CO.CDP.UserManagement.Infrastructure/Services/ | ❌ Absent |
| AddStackExchangeRedisCache() | ✅ Registered | ❌ Not called |
| AddUserManagementCaching() DI extension | ✅ ServiceCollectionExtensions.cs | ❌ Absent |
| 15-minute TTL | ✅ Hardcoded in CachedClaimsService | ❌ N/A |

---

## Issues Found

### ISSUE-3C-01: IClaimsCacheService Missing from CO-CDP
**Severity:** Critical
`IClaimsCacheService` is not defined in CO-CDP at `Libraries/CO.CDP.UserManagement.Core/Interfaces/`. Only `IJoinRequestOrchestrationService` and `IMembershipAuthorizationGuard` exist there. The interface exists only in `inventur-GCGS-Central-Digital-Platform/Libraries/CO.CDP.UserManagement.Core/Interfaces/IClaimsCacheService.cs`. Handlers and use cases in CO-CDP that reference this interface will fail at DI resolution time.

### ISSUE-3C-02: CachedClaimsService Decorator Missing from CO-CDP
**Severity:** Critical
The `CachedClaimsService` decorator (which wraps `IClaimsService` with `IDistributedCache` and a 15-minute TTL) is absent from CO-CDP. It exists only at `inventur-GCGS-Central-Digital-Platform/Services/CO.CDP.UserManagement.Infrastructure/Services/CachedClaimsService.cs`.

### ISSUE-3C-03: Redis Not Registered in Program.cs
**Severity:** Critical
`IDistributedCache` is not registered in `CO.CDP.Organisation.WebApi/Program.cs`. The `Microsoft.Extensions.Caching.StackExchangeRedis` NuGet package is present in the `.csproj` but `AddStackExchangeRedisCache()` is never called.

### ISSUE-3C-04: No TTL Configuration
**Severity:** High
TTL configuration is absent from the AppRegistry / Organisation.WebApi area of CO-CDP entirely. There is no `appsettings.json` entry or `IOptions<ClaimsCacheOptions>` type for configuring cache duration per environment.

### ISSUE-3C-05: DI Decorator Registration Absent
**Severity:** Critical
The `AddUserManagementCaching()` extension method that wires `IClaimsCacheService -> CachedClaimsService` in DI is not present in CO-CDP. It exists only in the inventur repo `ServiceCollectionExtensions.cs`.

### ISSUE-3C-06: IClaimsService Missing from CO-CDP
**Severity:** Critical
`IClaimsService` (the inner service that `CachedClaimsService` wraps) is also absent from CO-CDP's `UserManagement.Core` library. Without this interface, `CachedClaimsService` cannot be ported without also adding the concrete `ClaimsService` implementation.

---

## Recommendations

1. Port `IClaimsCacheService` and `IClaimsService` interfaces from `inventur-GCGS-Central-Digital-Platform/Libraries/CO.CDP.UserManagement.Core/Interfaces/` into `CO-CDP/Libraries/CO.CDP.UserManagement.Core/Interfaces/`.
2. Port `CachedClaimsService` from the inventur repo into `CO-CDP/Services/CO.CDP.UserManagement.Infrastructure/Services/CachedClaimsService.cs` and implement a concrete `IClaimsService` backed by the AppRegistry `/api/claims/{userPrincipalId}` endpoint.
3. Add `AddStackExchangeRedisCache()` call to `CO.CDP.Organisation.WebApi/Program.cs`, wired to the ElastiCache/Redis connection string (mirrors the pattern already used in `CO.CDP.RegisterOfCommercialTools.WebApi/Program.cs`).
4. Register `IClaimsCacheService -> CachedClaimsService` in DI using the decorator pattern: `services.AddScoped<IClaimsService, ClaimsService>(); services.AddScoped<IClaimsCacheService, CachedClaimsService>()`.
5. Make the cache TTL configurable via `appsettings.json` rather than hardcoding 15 minutes, so it can be tuned per environment (e.g. shorter in dev/staging, longer in production).
6. Confirm the AppRegistry `/api/claims/{userPrincipalId}` endpoint is the data source for `IClaimsService.GetUserClaimsAsync()` to close the known token-enrichment gap noted in project memory (also addressed in Phase 3B recommendations).
