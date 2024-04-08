# GCGS Central Digital Platform

## Development

Import the project to your favourite IDE to build and run tests from there.

Alternatively, use the `dotnet` command or the following make targets to build and run tests:

```bash
make build
make test
```

Any dotnet tools used by the project are [installed locally](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools#install-a-local-tool).

## Documentation

For technical documentation see [Docs](docs/index.adoc).

## Design Decisions

For important design decisions that were made as the project evolved see the [decision log](docs/decisions/index.adoc).

## Docker

While local development is done within our IDE of choice, the Docker configuration is provided to conveniently start
all the services at once.

First, we need to build all Docker containers:

```bash
make build-docker
```

To start all Docker services:

```bash
make up
```

The first run creates `compose.override.yml` that can be used to override service configuration locally.

By default service and application ports are mapped as follows:

* OrganisationApp - - http://localhost/
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

