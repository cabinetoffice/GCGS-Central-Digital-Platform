# Organisation Information

## Docker

While local development is done within our IDE of choice, the Docker configuration is provided to conveniently start
all the services at once.

First, we need to build all Docker containers:

```bash
make build
```

To start all Docker services:

```bash
make up
```

The first run creates `compose.override.yml` that can be used to override service configuration locally.

By default service ports are mapped as follows:

* Tenant - http://localhost:8080/swagger/
* Organisation - http://localhost:8082/swagger/
* Person - http://localhost:8084/swagger/
* Forms - http://localhost:8086/swagger/
* Data Sharing - http://localhost:8088/swagger/

Stop all Docker services:

```bash
make down
```
