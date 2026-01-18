# Application Registry API

A comprehensive multi-tenant authorization system for managing organisations, applications, permissions, roles, and user assignments.

## Architecture

The solution is structured in three layers:

### 1. Core Layer (`CO.CDP.ApplicationRegistry.Core`)
Domain entities, interfaces, and business logic contracts.

**Entities:**
- `Organisation` - Organisations with CDP integration
- `Application` - Applications that can be enabled for organisations
- `ApplicationPermission` - Granular permissions within applications
- `ApplicationRole` - Roles that bundle permissions
- `UserOrganisationMembership` - User membership in organisations
- `OrganisationApplication` - Applications enabled for organisations
- `UserApplicationAssignment` - User assignments to applications with roles

**Key Features:**
- All entities implement `ISoftDelete` and `IAuditable`
- Automatic audit tracking (CreatedAt/By, ModifiedAt/By)
- Soft delete support

### 2. Infrastructure Layer (`CO.CDP.ApplicationRegistry.Infrastructure`)
Data access, EF Core configurations, and service implementations.

**Features:**
- PostgreSQL database with EF Core
- Repository pattern with Unit of Work
- Audit interceptor for automatic timestamp/user tracking
- Redis-backed caching (15-minute TTL for claims)
- Fallback to in-memory cache for development

### 3. API Layer (`CO.CDP.ApplicationRegistry.Api`)
ASP.NET Core Web API with REST endpoints.

**Endpoints:**
- `/api/organisations` - Organisation management
- `/api/applications` - Application management
- `/api/claims/users/{userId}` - User claims resolution
- `/health` - Health check endpoint

**Security:**
- JWT Bearer authentication
- Authorization on all endpoints
- Current user tracking via HTTP context

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 12+
- Redis (optional, for production caching)

### Configuration

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ApplicationRegistryDatabase": "Host=localhost;Port=5432;Database=application_registry;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "Authentication": {
    "Authority": "https://your-identity-provider.com",
    "Audience": "application-registry-api"
  }
}
```

### Database Migrations

```bash
cd Services/CO.CDP.ApplicationRegistry.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../CO.CDP.ApplicationRegistry.Api
dotnet ef database update --startup-project ../CO.CDP.ApplicationRegistry.Api
```

### Running the API

```bash
cd Services/CO.CDP.ApplicationRegistry.Api
dotnet run
```

The API will be available at `https://localhost:5001` (or configured port).

### Running Tests

```bash
cd Services/CO.CDP.ApplicationRegistry.UnitTests
dotnet test
```

## Key Features

### Phase 1: Core Functionality
- Organisation and Application CRUD operations
- Slug generation for organisations
- Unique constraint validation (ClientId, Slug, CDP GUID)

### Phase 2A: Permissions & Roles
- Permission management scoped to applications
- Role creation with permission assignment
- Many-to-many relationships

### Phase 2B: Organisation-Application Linking
- Enable/disable applications for organisations
- Track enablement/disablement metadata
- Query applications by user

### Phase 2C: User Assignments
- Assign users to applications within organisations
- Multi-role support per assignment
- Assignment lifecycle tracking

### Phase 3A: Claims Resolution
- Complete claims structure for users
- Organisation memberships with roles
- Application assignments with roles and permissions
- Efficient query with EF Core includes

### Phase 3B: Caching
- Decorator pattern for claims caching
- 15-minute TTL
- Redis for production, in-memory for development
- Cache invalidation API

### Phase 3C: Current User Context
- HTTP context accessor for user identification
- Automatic audit field population
- Integration with JWT claims

## API Examples

### Create Organisation
```http
POST /api/organisations
Content-Type: application/json

{
  "cdpOrganisationGuid": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Acme Corporation",
  "isActive": true
}
```

### Get User Claims
```http
GET /api/claims/users/user-principal-id-123
Authorization: Bearer {token}
```

Response:
```json
{
  "userPrincipalId": "user-principal-id-123",
  "organisationMemberships": [
    {
      "organisationId": 1,
      "organisationName": "Acme Corporation",
      "organisationSlug": "acme-corporation",
      "organisationRole": "Admin",
      "applicationAssignments": [
        {
          "applicationId": 1,
          "applicationName": "Forms Service",
          "clientId": "forms-service-client",
          "roles": ["Admin", "Editor"],
          "permissions": ["create", "read", "update", "delete"]
        }
      ]
    }
  ]
}
```

## Database Schema

The database uses snake_case naming convention and includes:
- Composite unique indexes
- Foreign key constraints with cascade delete
- Soft delete query filters
- Audit columns on all tables
- Many-to-many join tables for roles/permissions and assignments/roles

## Testing

Comprehensive unit tests using:
- xUnit
- Moq for mocking
- FluentAssertions for readable assertions

Test coverage includes:
- Service layer business logic
- Repository operations
- Claims resolution
- Slug generation
- Exception handling
