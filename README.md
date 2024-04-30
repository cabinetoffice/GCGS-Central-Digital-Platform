# GCGS Central Digital Platform

## Development

Import the project to your favourite IDE to build and run tests from there.

Alternatively, use the `dotnet` command or the following make targets to build and run tests:

```bash
make build
make test
```

Any dotnet tools used by the project are [installed locally](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools#install-a-local-tool).

The database is one of the services defined in Docker Compose. It can be started on its own and used with the IDE setup.
First, containers need to be built:

```bash
make build-docker
```

The database can be then started together with migrations:

```bash
docker compose up -d db tenant-migrations organisation-migrations
```

### Configuration

The application is mostly configured to start with a fresh repository checkout.
Secrets that are not safe to be committed to the repository are managed with the
[Secrets Manager](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=linux#secret-manager):

* `OneLogin:Authority`
* `OneLogin:ClientId`
* `OneLogin:PrivateKey`

These can be set within the IDE, or with the dotnet command:

```bash
dotnet user-secrets set --project Frontend/CO.CDP.OrganisationApp/CO.CDP.OrganisationApp.csproj OneLogin:Authority "https://oidc.example.com"
dotnet user-secrets set --project Frontend/CO.CDP.OrganisationApp/CO.CDP.OrganisationApp.csproj OneLogin:ClientId "client-id"
dotnet user-secrets set --project Frontend/CO.CDP.OrganisationApp/CO.CDP.OrganisationApp.csproj OneLogin:PrivateKey "-----BEGIN RSA PRIVATE KEY-----
SECRET KEY
-----END RSA PRIVATE KEY-----"
```

**Never commit secrets to the repository.**

## Documentation

For technical documentation see [Docs](docs/index.adoc).

## Design Decisions

For important design decisions that were made as the project evolved see the [decision log](docs/decisions/index.adoc).

## Docker

While local development is done within our IDE of choice, the Docker configuration is provided to conveniently start
all the services at once.

### Building containers

First, we need to build all Docker containers:

```bash
make build-docker
```

### Configuration

Run `make compose.override.yml` to generate the default configuration.

One login details need to be provided to the `organisation-app` in `compose.override.yml` as environment variables:

* `OneLogin__Authority`
* `OneLogin__ClientId`
* `OneLogin__PrivateKey`

### Starting services

To start all Docker services:

```bash
make up
```

The first run creates `compose.override.yml` that can be used to override service configuration locally.

By default service and application ports are mapped as follows:

* OrganisationApp - - http://localhost:8090/
* Tenant - http://localhost:8080/swagger/
* Organisation - http://localhost:8082/swagger/
* Person - http://localhost:8084/swagger/
* Forms - http://localhost:8086/swagger/
* Data Sharing - http://localhost:8088/swagger/
* PostgreSQL database - :5432

In order to prevent selected service(s) from starting, configure the number of replicate in `compose.override.yml` to 0:

```yaml
# ...
  person:
    deploy:
      replicas: 0
    # ...
```

Stop all Docker services:

```bash
make down
```

To only start the database and apply migrations run:


```bash
make db
```

