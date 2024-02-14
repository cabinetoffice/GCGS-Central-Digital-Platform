
build:
	@docker compose build
.PHONY: build

up: compose.override.yml
	@docker compose up -d
	@docker compose ps
.PHONY: up

down:
	@docker compose down
.PHONY: down

ps:
	@docker compose ps
.PHONY: ps

test:
	@dotnet test
.PHONY: test

define COMPOSE_OVERRIDE_YML
version: '3'
services:
  tenant:
    ports:
      - '8080:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
  organisation:
    ports:
      - '8082:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
  person:
    ports:
      - '8084:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
  forms:
    ports:
      - '8086:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
  data-sharing:
    ports:
      - '8088:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
endef

export COMPOSE_OVERRIDE_YML
compose.override.yml:
	@echo "$$COMPOSE_OVERRIDE_YML" > compose.override.yml
