# End-to-End Authentication Flow

This document describes the complete authentication and authorisation flow across the CDP platform, incorporating application-level role enforcement introduced by the Application Registry.

---

## Overview

```
User
 │
 ▼
GOV.UK One Login  (OIDC sign-in)
 │  id_token (sub = user URN)
 ▼
CO.CDP.Organisation.Authority  (token exchange)
 │  calls Organisation API for cdp_claims enrichment
 ▼
CO.CDP.Organisation.WebApi  (GET /organisations/claims/users/{urn})
 │  queries OrganisationInformation DB
 ▼
CDP JWT issued to client
 │  contains: sub, channel, role, cdp_claims
 ▼
Protected API endpoint
 │
 ▼
ApplicationScopeAuthorizationHandler  (evaluates cdp_claims)
 │
 ▼
Access granted / denied
```

---

## Step 1 — GOV.UK One Login

The user authenticates via GOV.UK One Login (OIDC). One Login issues a signed JWT containing:

| Claim | Value |
|---|---|
| `sub` | User URN e.g. `urn:fdc:gov.uk:2022:abc123` |
| `iss` | One Login issuer URL |

---

## Step 2 — Authority service validates the One Login token

**Service:** `CO.CDP.Organisation.Authority`  
**Class:** `TokenService.ValidateOneLoginToken()`

- Fetches One Login's JWKS (public signing keys) via `IConfigurationService`
- Validates signature, issuer, and token lifetime using `TokenValidationParameters`
- If the signing key is not found, retries **once** with a refreshed JWKS (handles key rotation)
- Extracts the `sub` claim as the user's URN

---

## Step 3 — Claims enrichment via Organisation API

**Feature flag:** `Features:ClaimsApiEnabled`

When enabled, the Authority service calls the Organisation API as a service account to fetch the user's application role claims before issuing the CDP token.

**Request:**
```
GET /organisations/claims/users/{encodedUrn}
Authorization: Bearer <service-account-token>
```

The HTTP client (`OrganisationApiHttpClient`) is registered with `ServiceAccountTokenHandler` as a delegating handler — all service-to-service calls are authenticated with a service account token, not the user's token.

**`GetUserClaimsUseCase`** on the Organisation API:

1. Looks up the `Person` record by URN
2. Loads all `OrganisationPerson` memberships (organisations the user belongs to and their org-level role/scope)
3. Loads all active `UserApplicationAssignment` records for this person, joined through `OrganisationApplication → Application → ApplicationRole → ApplicationPermission`
4. Groups assignments by organisation and maps to the response shape

**Response shape:**
```json
{
  "userPrincipalId": "urn:fdc:gov.uk:2022:abc123",
  "organisations": [
    {
      "organisationId": "3fa85f64-...",
      "organisationName": "ACME Ltd",
      "organisationRole": "Admin",
      "applications": [
        {
          "applicationId": "7cb12a31-...",
          "applicationName": "Find a Tender",
          "clientId": "fts",
          "roles": ["buyer"],
          "permissions": ["create-notice", "publish-notice"]
        }
      ]
    }
  ]
}
```

> **Graceful degradation:** if the Organisation API is unavailable or returns non-2xx, the CDP token is still issued — the `cdp_claims` claim is simply omitted. Endpoints that require application-scope checks will deny access, but the login itself is not blocked.

---

## Step 4 — CDP access token issued

**`TokenService.CreateToken(urn)`** → **`CreateAccessToken()`**

The CDP access token (RS256 JWT) is assembled with:

| Claim | Source |
|---|---|
| `sub` | User URN (from One Login `sub`) |
| `channel` | `"one-login"` (hardcoded) |
| `role` | Person's scopes from `Persons` table in OrganisationInformation DB |
| `cdp_claims` | JSON-typed claim containing all organisation memberships and application assignments (see Step 3) |

A refresh token is also generated as a PBKDF2 hash stored in the `AuthorityRepository` (PostgreSQL).

Token expiry:
- Access token: **1 hour**
- Refresh token: **24 hours**

---

## Step 5 — Client calls a protected endpoint

The client presents the CDP JWT as a Bearer token:

```
GET /organisations/{orgId}/...
Authorization: Bearer <cdp-jwt>
```

---

## Step 6 — Application-scoped authorisation

**Class:** `ApplicationScopeAuthorizationHandler`  
**Requirement:** `ApplicationScopeAuthorizationRequirement(clientId, roles?, permissions?)`

The handler evaluates the `cdp_claims` JWT claim against the requirement:

```
1. channel != "one-login"          → PASS  (service accounts bypass)
2. role contains "SuperAdmin"      → PASS  (platform admins bypass)
3. cdp_claims missing or invalid   → FAIL
4. Extract organisationId from URL path: /organisations/{guid}/...
5. Find org claim matching organisationId     → FAIL if not found
6. Find app claim matching clientId           → FAIL if not found
7. No roles/permissions required              → PASS  (assigned = sufficient)
8. Required roles ∩ user roles ≠ ∅           → PASS
9. Required permissions ∩ user permissions ≠ ∅ → PASS
10. Otherwise                                 → FAIL
```

---

## Data model

```
Application
├── ClientId          (unique identifier used in authorisation requirements)
├── Name
├── IsActive
├── Roles[]           → ApplicationRole
│     ├── Name
│     ├── IsActive
│     └── Permissions[] → ApplicationPermission (Name)
└── Permissions[]     → ApplicationPermission

Organisation
└── OrganisationApplication   (links Application to Organisation)
      ├── IsActive
      └── UserApplicationAssignment  (links Person to OrgApplication)
            ├── IsActive
            ├── AssignedBy / AssignedAt
            ├── RevokedBy / RevokedAt
            └── Roles[]  → ApplicationRole  (roles granted to this person in this org)
```

---

## Configuration

### Authority service (`appsettings.json`)

```json
{
  "Features": {
    "ClaimsApiEnabled": true
  },
  "OrganisationApiService": "https://organisation-api"
}
```

### Organisation API (`appsettings.json`)

The `/organisations/claims/users/{urn}` endpoint is restricted to `ServiceAccount` channel authentication only. Non-service-account callers receive 401.

---

## Key design decisions

| Decision | Rationale |
|---|---|
| Claims embedded in JWT | Avoids per-request DB lookups on every protected endpoint |
| Graceful degradation on claims fetch | Login is never blocked by Organisation API unavailability |
| Service account for service-to-service | User tokens are not forwarded; Authority uses its own identity |
| `SuperAdmin` scope bypasses app-scope checks | Platform admins need cross-organisation access |
| `organisationId` extracted from URL path | Avoids requiring callers to pass it explicitly in a header |
| One-time JWKS retry on key-not-found | Handles One Login key rotation without restart |
