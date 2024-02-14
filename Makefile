build:
	@dotnet restore
	@dotnet build
.PHONY: build

test: TEST_OPTIONS ?= ""
test:
	@dotnet test $(TEST_OPTIONS)
.PHONY: test

build-docker:
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

define COMPOSE_OVERRIDE_YML
version: '3'
services:
  tenant:
    ports:
      - "$${CDP_TENANT_PORT:-8080}:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
  organisation:
    ports:
      - '$${CDP_ORGANISATION_PORT:-8082}:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
  person:
    ports:
      - '$${CDP_PERSON_PORT:-8084}:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
  forms:
    ports:
      - '$${CDP_FORMS_PORT:-8086}:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
  data-sharing:
    ports:
      - '$${CDP_DATA_SHARING_PORT:-8088}:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
endef

export COMPOSE_OVERRIDE_YML
compose.override.yml:
	@echo "$$COMPOSE_OVERRIDE_YML" > compose.override.yml
