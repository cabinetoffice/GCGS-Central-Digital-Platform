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

OpenAPI: build
	@mkdir -p OpenAPI
	cp ./Services/CO.CDP.Tenant.WebApi/OpenAPI/CO.CDP.Tenant.WebApi.json OpenAPI/Tenant.json
	cp ./Services/CO.CDP.DataSharing.WebApi/OpenAPI/CO.CDP.DataSharing.WebApi.json OpenAPI/DataSharing.json
	cp ./Services/CO.CDP.Organisation.WebApi/OpenAPI/CO.CDP.Organisation.WebApi.json OpenAPI/Organisation.json
	cp ./Services/CO.CDP.Person.WebApi/OpenAPI/CO.CDP.Person.WebApi.json OpenAPI/Person.json
	cp ./Services/CO.CDP.Forms.WebApi/OpenAPI/CO.CDP.Forms.WebApi.json OpenAPI/Forms.json

define COMPOSE_OVERRIDE_YML
version: '3'
services:
  db:
    ports:
      - "$${CDP_DB_PORT:-5432}:5432"
    deploy:
      replicas: 1
  organisation-app:
    ports:
      - "$${CDP_ORGANISATION_APP_PORT:-80}:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    deploy:
      replicas: 1
  tenant:
    ports:
      - "$${CDP_TENANT_PORT:-8080}:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    deploy:
      replicas: 1
  organisation:
    ports:
      - '$${CDP_ORGANISATION_PORT:-8082}:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    deploy:
      replicas: 1
  person:
    ports:
      - '$${CDP_PERSON_PORT:-8084}:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    deploy:
      replicas: 1
  forms:
    ports:
      - '$${CDP_FORMS_PORT:-8086}:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    deploy:
      replicas: 1
  data-sharing:
    ports:
      - '$${CDP_DATA_SHARING_PORT:-8088}:8080'
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    deploy:
      replicas: 1
endef

export COMPOSE_OVERRIDE_YML
compose.override.yml:
	@echo "$$COMPOSE_OVERRIDE_YML" > compose.override.yml
