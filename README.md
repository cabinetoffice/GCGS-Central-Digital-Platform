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

The first time, `compose.override.yml` needs to be generated and configured (see the Docker section).

The database can be then started together with migrations (`make db`):

```bash
docker compose up -d db organisation-information-migrations
```

### Mixing services started by IDE with Docker

During development, testing, or debugging it's often useful to run some services with Docker,
while others with an IDE.

To do that, first start all the services with Docker. Remember to override environment variables for services
that you plan to run in an IDE:

```bash
OrganisationService=http://host.docker.internal:8082 docker compose up -d
```

In the above example, we pointed the Organisation service to the one we plan to start with the IDE.
`host.docker.internal` is used to connect from Docker containers to the host machine.

Next, stop selected services in Docker:

```bash
docker compose stop organisation
```

Finally, start the selected services in the IDE.

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

The authority service also needs to be configured with:

* `PublicKey`
* `PrivateKey`
* `OneLogin__Authority`

The `make generate-authority-keys` command generates a pair of public and private keys that
can be used with `PublicKey` and `PrivateKey`. Make sure to copy the contents of both files and not the path.

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

