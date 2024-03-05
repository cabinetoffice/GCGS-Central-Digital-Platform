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

CDP_ORGANISATION_APP_PORT ?= 80
CDP_TENANT_PORT ?= 8080
CDP_ORGANISATION_PORT ?= 8082
CDP_PERSON_PORT ?= 8084
CDP_FORMS_PORT ?= 8086
CDP_DATA_SHARING_PORT ?= 8088
OpenAPI: up
	@mkdir -p OpenAPI
	@curl -sL http://localhost:$(CDP_TENANT_PORT)/swagger/v1/swagger.json > OpenAPI/TenantApi.json
	@curl -sL http://localhost:$(CDP_TENANT_PORT)/swagger/v1/swagger.yaml > OpenAPI/TenantApi.yaml
	@curl -sL http://localhost:$(CDP_ORGANISATION_PORT)/swagger/v1/swagger.json > OpenAPI/OrganisationApi.json
	@curl -sL http://localhost:$(CDP_ORGANISATION_PORT)/swagger/v1/swagger.yaml > OpenAPI/OrganisationApi.yaml
	@curl -sL http://localhost:$(CDP_PERSON_PORT)/swagger/v1/swagger.json > OpenAPI/PersonApi.json
	@curl -sL http://localhost:$(CDP_PERSON_PORT)/swagger/v1/swagger.yaml > OpenAPI/PersonApi.yaml
	@curl -sL http://localhost:$(CDP_FORMS_PORT)/swagger/v1/swagger.json > OpenAPI/FormsApi.json
	@curl -sL http://localhost:$(CDP_FORMS_PORT)/swagger/v1/swagger.yaml > OpenAPI/FormsApi.yaml
	@curl -sL http://localhost:$(CDP_DATA_SHARING_PORT)/swagger/v1/swagger.json > OpenAPI/DataSharingApi.json
	@curl -sL http://localhost:$(CDP_DATA_SHARING_PORT)/swagger/v1/swagger.yaml > OpenAPI/DataSharingApi.yaml

define COMPOSE_OVERRIDE_YML
version: '3'
services:
  organisation-app:
    ports:
      - "$${CDP_ORGANISATION_APP_PORT:-80}:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
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
