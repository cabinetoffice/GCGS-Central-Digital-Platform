# CO-CDP Local Development Runbook

> Last updated: June 2026 · Branch: `feature/app-roles`

---

## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [One-time Setup](#2-one-time-setup)
3. [Quick Start (Full Docker Stack)](#3-quick-start-full-docker-stack)
4. [Service Inventory & Port Map](#4-service-inventory--port-map)
5. [Running Individual Services Locally](#5-running-individual-services-locally)
6. [Database Operations](#6-database-operations)
7. [Running Tests](#7-running-tests)
8. [Running E2E (Playwright) Tests](#8-running-e2e-playwright-tests)
9. [Common Make Targets](#9-common-make-targets)
10. [Troubleshooting](#10-troubleshooting)

---

## 1. Prerequisites

| Tool | Version | Notes |
|---|---|---|
| **.NET SDK** | 8.0+ | `dotnet --version` |
| **Docker Desktop** | 4.x+ | Must be running before any `make up` |
| **Docker Compose v2** | Bundled with Docker Desktop | `docker compose version` |
| **Node.js** | 18+ | Only for Playwright browser install |
| **make** | GNU Make | Pre-installed on macOS via Xcode tools |

**macOS: set Docker on your `PATH` for terminal sessions:**

```bash
export PATH="$PATH:/Applications/Docker.app/Contents/Resources/bin"
```

Add this to `~/.zshrc` or `~/.bash_profile` to make it permanent.

---

## 2. One-time Setup

### 2a. Clone and build

```bash
cd /Users/sammywilliams/RiderProjects/CO-CDP

# Restore and build .NET solution
make build
```

### 2b. Create `compose.override.yml`

The override file sets ports exposed to localhost and environment-specific config. **Never commit it** — it contains secrets.

```bash
make render-compose-override       # copies compose.override.yml.template → compose.override.yml
```

### 2c. Set required secrets in `compose.override.yml`

Open `compose.override.yml` and fill in the four variables that have **no defaults**:

```yaml
services:
  organisation-app:
    environment:
      OneLogin__ClientId: "${SIRSI_ONELOGIN_CLIENTID}"          # ← set this
      OneLogin__PrivateKey: "${SIRSI_ONELOGIN_PRIVATEKEY}"       # ← set this (PEM format)
      CompaniesHouse__User: "${SIRSI_ORGANISATION_APP_COMPANIESHOUSE_USER}"            # ← set this
      CharityCommission__SubscriptionKey: "${SIRSI_ORGANISATION_APP_CHARITYCOMMISSION_SUBSCRIPTIONKEY}" # ← set this
  authority:
    environment:
      PrivateKey: "${SIRSI_ONELOGIN_PRIVATEKEY}"                 # ← same key as above
```

Either export them as shell environment variables before running Docker:

```bash
export SIRSI_ONELOGIN_CLIENTID="your-client-id"
export SIRSI_ONELOGIN_PRIVATEKEY="$(cat path/to/private-key.pem)"
export SIRSI_ORGANISATION_APP_COMPANIESHOUSE_USER="your-ch-user"
export SIRSI_ORGANISATION_APP_CHARITYCOMMISSION_SUBSCRIPTIONKEY="your-cc-key"
```

Or replace the `${VAR}` placeholders directly in `compose.override.yml` with the real values.

> **Generating a local authority key** (if you don't have one):
> ```bash
> make generate-authority-keys
> # Output: ./terragrunt/secrets/authority-private-key.pem
> ```

### 2d. Build all Docker images

```bash
# Parallel build (faster, needs ~8GB RAM)
make build-docker-parallel VERSION=local

# OR sequential build (safer on limited RAM)
make build-docker VERSION=local
```

This builds every Dockerfile target: authority, tenant, organisation, person, forms, data-sharing, entity-verification, organisation-app, commercial-tools-app, commercial-tools-api, av-scanner, scheduled-worker, outbox-processors, and both migration runners.

---

## 3. Quick Start (Full Docker Stack)

```bash
# 1. Start everything
make up

# 2. Wait for all services to be healthy (~60 seconds)
make verify-up

# 3. Access the application
open http://localhost:8090    # Organisation App (main UI)
open http://localhost:8082/swagger    # Organisation WebApi Swagger
open http://localhost:8092/swagger    # Authority Swagger
```

**Tear down:**

```bash
make down       # remove containers and network
make stop       # stop containers but keep state
```

---

## 4. Service Inventory & Port Map

### Infrastructure

| Container | Image | `localhost` Port | Purpose |
|---|---|---|---|
| `co-cdp-gateway` | `nginx:1.27` | Routes all services | Reverse proxy, single entry point |
| `co-cdp-db` | `postgres:16.3` | **5432** | Shared PostgreSQL database |
| `co-cdp-redis` | `redis:7.4.1` | **6379** | Session / distributed cache |
| `co-cdp-mongodb` | `mongo:7.0` | **27017** | ApplicationRegistry data store |
| `co-cdp-localstack` | `localstack/localstack:3.5` | **4566** | AWS services: SQS, S3, SSM, Logs |
| `co-cdp-clamav-rest-1` | `ajilaag/clamav-rest` | **9000 / 9443** | AV scanning REST endpoint |

### Application Services (all routed through gateway on port 8080)

| Container | Dockerfile Target | `localhost` Port | Service |
|---|---|---|---|
| `co-cdp-organisation-app` | `final-organisation-app` | **8090** | Main organisation frontend (Razor Pages) |
| `co-cdp-authority` | `final-authority` | **8092** | OAuth / OIDC token authority |
| `co-cdp-tenant` | `final-tenant` | **8080** | Tenant management API |
| `co-cdp-organisation` | `final-organisation` | **8082** | Organisation WebApi (incl. AppRegistry) |
| `co-cdp-person` | `final-person` | **8084** | Person management API |
| `co-cdp-forms` | `final-forms` | **8086** | Forms / supplier information API |
| `co-cdp-data-sharing` | `final-data-sharing` | **8088** | Data sharing / OCDS API |
| `co-cdp-entity-verification` | `final-entity-verification` | **8094** | Entity (Companies House / Charity) verification |
| `co-cdp-commercial-tools-app` | `final-commercial-tools-app` | **8192** | Commercial Tools frontend |
| `co-cdp-commercial-tools-api` | `final-commercial-tools-api` | **8184** | Commercial Tools API |

### Background Services (no public port)

| Container | Dockerfile Target | Purpose |
|---|---|---|
| `co-cdp-av-scanner-app` | `final-antivirus-app` | Antivirus scanning worker |
| `co-cdp-outbox-processor-organisation` | `final-outbox-processor-organisation` | Transactional outbox for organisation events |
| `co-cdp-outbox-processor-entity-verification` | `final-outbox-processor-entity-verification` | Transactional outbox for EV events |
| `co-cdp-scheduled-worker` | `final-scheduled-worker` | Scheduled background jobs |

### One-shot Migration Containers

| Container | Dockerfile Target | Runs |
|---|---|---|
| `co-cdp-organisation-information-migrations` | `migrations-organisation-information` | EF Core migrations for main schema |
| `co-cdp-entity-verification-migrations` | `migrations-entity-verification` | EF Core migrations for EV schema |

### LocalStack Services

| AWS Service | LocalStack endpoint | Used by |
|---|---|---|
| **SQS** | `http://localhost:4566` | Outbox processors, entity verification, av-scanner |
| **S3** | `http://localhost:4566` | Forms, data-sharing, commercial-tools |
| **SSM** | `http://localhost:4566` | Config / secrets management |
| **CloudWatch Logs** | `http://localhost:4566` | Structured logging |

**S3 buckets:** `cdp-staging-bucket.s3`, `cdp-permanent-bucket.s3`  
**SQS queues:** `entity-verification.fifo`, `organisation.fifo`, `av-scanner.fifo`

### Database Connection

```
Host:     localhost (or `db` inside Docker network)
Port:     5432
Database: cdp
User:     cdp_user
Password: cdp123
```

### MongoDB Connection

```
URI:      mongodb://localhost:27017
Database: app_registry
```

---

## 5. Running Individual Services Locally

For faster iteration you can run one service locally with `dotnet run` while the rest of the stack runs in Docker. The `compose.override.yml.template` includes commented-out `host.docker.internal` lines for each service — uncomment the ones you want to replace with a local process.

### Example: run Organisation WebApi locally

**1. Uncomment in `compose.override.yml`:**

```yaml
services:
  gateway:
    environment:
      CDP_ORGANISATION_HOST: 'http://host.docker.internal:58082'
```

**2. Start infrastructure in Docker (skip organisation service):**

```bash
export PATH="$PATH:/Applications/Docker.app/Contents/Resources/bin"
docker compose up -d db redis localstack mongodb authority tenant person
```

**3. Run the service locally:**

```bash
cd Services/CO.CDP.Organisation.WebApi
dotnet run --launch-profile https
# or with explicit URLs:
ASPNETCORE_URLS="http://localhost:58082" dotnet run
```

The gateway will forward `http://localhost:8082` → `http://host.docker.internal:58082` → your local process.

### Example: run OrganisationApp locally

```bash
cd Frontend/CO.CDP.OrganisationApp
dotnet run --launch-profile https
# Available at http://localhost:58090 or the configured launch URL
```

Update `compose.override.yml` to point `CDP_ORGANISATION_APP_HOST` at `host.docker.internal:58090`.

### Key `appsettings.Development.json` values

All `appsettings.Development.json` files are pre-configured for a local stack. No edits needed provided:
- Docker stack is running on default ports
- You've set the required environment variables (see §2c)

---

## 6. Database Operations

### Run migrations only

```bash
make db    # starts db container + runs both migration bundles
```

### Manual migration (EF Core CLI)

```bash
cd Services/CO.CDP.OrganisationInformation.Persistence
dotnet ef database update -- --connectionString "Server=localhost;Database=cdp;User Id=cdp_user;Password=cdp123;"
```

### Backup and restore

```bash
# Backup
make db-dump                        # creates timestamped .sql in project root

# Restore
make db-restore FILE=mybackup.sql   # restores from a .sql file

# Restore test data
make db-restore FILE=testAutomation1.sql
```

### Connect directly

```bash
# psql
psql -h localhost -p 5432 -U cdp_user -d cdp

# MongoDB shell
docker exec -it co-cdp-mongodb mongosh app_registry
```

---

## 7. Running Tests

### Unit and integration tests

```bash
# All tests
make test

# Specific project
dotnet test Services/CO.CDP.Organisation.WebApi.Tests

# With filter
dotnet test --filter "FullyQualifiedName~ApplicationRegistry"

# Note: Docker must be running for Testcontainers-based integration tests
# (tests that spin up PostgreSQL in Docker automatically)
```

### Known test categories

| Filter | Coverage |
|---|---|
| `Category=ApplicationRegistry` | AppRegistry UI Playwright tests |
| `FullyQualifiedName~Organisation` | Organisation WebApi unit tests |
| `FullyQualifiedName~Role` | Role model tests |
| `FullyQualifiedName~Claims` | Claims service tests |
| `FullyQualifiedName~Authentication` | Auth library tests |

---

## 8. Running E2E (Playwright) Tests

### Configure credentials

E2E tests require a real GOV.UK One Login integration test account. Add credentials to `E2ETests/appsettings.Development.json` (create it — it is gitignored):

```json
{
  "TestSettings": {
    "BaseUrl": "http://localhost:8090",
    "ApiUrl": "http://localhost:8082",
    "Email": "your-test-email@example.com",
    "Password": "your-test-password",
    "SecretKey": "your-totp-secret",
    "Headless": false,
    "DatabaseConnectionString": "Server=localhost;Database=cdp;User Id=cdp_user;Password=cdp123;",
    "TestSupportAdminEmail": "admin@example.com",
    "TestSupportAdminPassword": "admin-password",
    "TestSupportAdminSecretKey": "admin-totp-secret"
  }
}
```

### Run against local stack (host machine)

```bash
cd E2ETests

# Install Playwright browsers (first time only)
dotnet build
pwsh bin/Debug/net9.0/playwright.ps1 install
# or: npx playwright install chromium

# Run all tests
dotnet test

# Run by category
dotnet test --filter "Category=ApplicationRegistry"
dotnet test --filter "Category=Users"
dotnet test --filter "Category=SuperAdmin"

# Run headed (watch the browser)
# Set Headless: false in appsettings.Development.json, then:
dotnet test --filter "Category=ApplicationRegistry"
```

### Run in Docker against full stack

```bash
# Start the full stack first
make up && make verify-up

# Run E2E tests in Docker (built-in make target)
make e2e-test

# OR manually:
docker build -t e2e-tests E2ETests/
docker run --rm \
  --network cdp-sirsi \
  -e TestSettings__BaseUrl=http://organisation-app:8080 \
  -e TestSettings__ApiUrl=http://organisation:8080 \
  -e TestSettings__Headless=true \
  -e TestSettings__Email="$E2E_EMAIL" \
  -e TestSettings__Password="$E2E_PASSWORD" \
  -e TestSettings__SecretKey="$E2E_SECRET" \
  -e TestSettings__DatabaseConnectionString="Server=db;Database=cdp;User Id=cdp_user;Password=cdp123;" \
  e2e-tests \
  dotnet test E2ETests.csproj --filter "Category=ApplicationRegistry"
```

### ApplicationRegistry test suite (new)

The Application Registry UI tests live in `E2ETests/Tests/Applications/`. They require:
- A Buyer organisation in the system
- At least one application registered in the AppRegistry MongoDB store
- Org members available for role assignment

| Test class | Tests |
|---|---|
| `ApplicationRegistryNavigationTests` | Page load, heading, link visibility, route registration |
| `ApplicationRegistryFunctionalTests` | Roles/Permissions tabs, assign user journey, edit roles, revoke |

---

## 9. Common Make Targets

```bash
make help                    # list all targets
make build                   # dotnet restore + build
make test                    # dotnet test (all)
make up                      # docker compose up -d (full stack)
make down                    # docker compose down
make stop                    # docker compose stop
make ps                      # docker compose ps
make verify-up               # wait for all containers healthy
make db                      # start db + run migrations
make db-dump                 # backup database to SQL file
make db-restore FILE=x.sql   # restore database from SQL file
make localstack              # start LocalStack only
make redis                   # start Redis only
make build-docker-parallel   # build all Docker images in parallel
make build-docker            # build all Docker images sequentially
make e2e-test                # run Playwright E2E tests in Docker
make render-compose-override # create compose.override.yml from template
make generate-authority-keys # generate RSA key pair for local authority
make OpenAPI                 # export OpenAPI specs from running services
```

---

## 10. Troubleshooting

### Docker not found

```bash
export PATH="$PATH:/Applications/Docker.app/Contents/Resources/bin"
```

### `ERR_TOO_MANY_REDIRECTS` in E2E tests

Credentials are missing or wrong. Check `appsettings.Development.json` in `E2ETests/`. The test framework uses real GOV.UK One Login — without valid credentials it will loop on the auth redirect.

### `ManyServiceProvidersCreatedWarning` in tests

When running more than ~20 WebApplicationFactory instances, EF Core raises this warning. The fix is in place for AppRegistry tests (uses `ConfigureInMemoryDbContext`) but if you see it in other test suites, add the same extension method call in the test factory setup.

### Service fails to start — `No such image`

Build the missing image:

```bash
docker compose build <service-name>
# e.g.:
docker compose build organisation
docker compose build organisation-app
```

Or rebuild all:

```bash
make build-docker-parallel
```

### Migrations not running / database errors

```bash
make db   # explicitly runs migration containers
# or force-recreate them:
docker compose up --force-recreate organisation-information-migrations
docker compose up --force-recreate entity-verification-migrations
```

### MongoDB connection refused

```bash
# Check MongoDB container status
docker compose ps mongodb

# View logs
docker compose logs mongodb

# Ping MongoDB
docker exec co-cdp-mongodb mongosh --eval "db.adminCommand('ping')"

# The Organisation WebApi needs MongoDB for AppRegistry endpoints.
# Connection string in compose: mongodb://mongodb:27017 (inside Docker)
# Connection string locally:    mongodb://localhost:27017
```

### LocalStack not ready

```bash
docker compose logs localstack
# Wait for "Ready" message. Takes 10-30s on first start.
# SQS queues are created automatically on startup.
```

### Port already in use

Check which process is on the port:

```bash
lsof -i :8090   # OrganisationApp
lsof -i :8082   # OrganisationWebApi
lsof -i :5432   # PostgreSQL
lsof -i :27017  # MongoDB
```

Override the port in `compose.override.yml`:

```yaml
services:
  gateway:
    ports:
      - "9090:8090"   # remap org-app to 9090 locally
```

### `docker compose up` fails with env var warnings

The warnings about `SIRSI_*` variables not being set are **expected** if you haven't exported them. They only matter for the `organisation-app` and `authority` services (OneLogin auth will break without them). All other services start fine.

### Rebuilding after code changes

```bash
# Rebuild specific service image only
docker compose build --no-cache organisation
docker compose build --no-cache organisation-app

# Then restart that service
docker compose up -d --no-build --force-recreate organisation
docker compose up -d --no-build --force-recreate organisation-app
```

---

## Appendix A — Full Dependency Graph

```
                    ┌─────────────────────────────────────────┐
                    │              gateway (nginx)              │
                    │         http://localhost:8090,8082,...    │
                    └───────────────────┬─────────────────────┘
                                        │
          ┌─────────────────────────────┼─────────────────────────────┐
          │                             │                             │
    organisation-app              organisation-api              authority
    (Frontend, :8090)          (WebApi+AppRegistry, :8082)      (:8092)
          │                             │                             │
          │                    ┌────────┼────────┐                   │
          │                    │        │        │                   │
          │                   db    mongodb  localstack              db
          │                (postgres) (mongo)  (AWS sim)
          │
     ┌────┴────┐
     │         │
    redis   authority
 (sessions)  (OAuth)
```

## Appendix B — Environment Variable Reference

| Variable | Required | Default | Used by |
|---|---|---|---|
| `SIRSI_ONELOGIN_CLIENTID` | **Yes** | — | organisation-app, authority |
| `SIRSI_ONELOGIN_PRIVATEKEY` | **Yes** | — | organisation-app, authority |
| `SIRSI_ORGANISATION_APP_COMPANIESHOUSE_USER` | **Yes** | — | organisation-app |
| `SIRSI_ORGANISATION_APP_CHARITYCOMMISSION_SUBSCRIPTIONKEY` | **Yes** | — | organisation-app |
| `IMAGE_VERSION` | No | `latest` | All docker compose build |
| `CDP_ORGANISATION_APP_PORT` | No | `8090` | gateway port mapping |
| `CDP_AUTHORITY_PORT` | No | `8092` | gateway port mapping |
| `CDP_ORGANISATION_PORT` | No | `8082` | gateway port mapping |
| `CDP_PERSON_PORT` | No | `8084` | gateway port mapping |
| `CDP_FORMS_PORT` | No | `8086` | gateway port mapping |
| `CDP_DATA_SHARING_PORT` | No | `8088` | gateway port mapping |
| `CDP_ENTITY_VERIFICATION_PORT` | No | `8094` | gateway port mapping |
| `CDP_COMMERCIAL_TOOLS_APP_PORT` | No | `8192` | gateway port mapping |
| `CDP_COMMERCIAL_TOOLS_API_PORT` | No | `8184` | gateway port mapping |
| `CDP_DB_PORT` | No | `5432` | postgres port mapping |
| `CDP_REDIS_PORT` | No | `6379` | redis port mapping |
| `CDP_MONGODB_PORT` | No | `27017` | mongodb port mapping |

## Appendix C — Swagger / OpenAPI Endpoints

All Swagger UIs are accessible when `Features__SwaggerUI=true` (default in Development):

| Service | URL |
|---|---|
| Organisation WebApi (incl. AppRegistry) | http://localhost:8082/swagger |
| Authority | http://localhost:8092/swagger |
| Tenant | http://localhost:8080/swagger |
| Person | http://localhost:8084/swagger |
| Forms | http://localhost:8086/swagger |
| Data Sharing | http://localhost:8088/swagger |
| Entity Verification | http://localhost:8094/swagger |
| Commercial Tools API | http://localhost:8184/swagger |

Export all specs at once:

```bash
make OpenAPI   # outputs to ./OpenAPI/*.json
```
