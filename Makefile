# Define Docker/ECR attributes
REPO_URL := 471112892058.dkr.ecr.eu-west-2.amazonaws.com
IMAGES := cdp-organisation-information-migrations cdp-data-sharing cdp-forms cdp-organisation-app cdp-organisation cdp-person cdp-tenant
TAGGED_IMAGES := $(addprefix cabinetoffice/,$(addsuffix :latest,$(IMAGES)))
TAGGED_REPO_IMAGES := $(addprefix $(REPO_URL)/,$(TAGGED_IMAGES))

# Extracts targets and their comments
help: ## List available commands
	@echo "Available commands:"
	@grep --no-filename -E '^[a-zA-Z0-9_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}'
.PHONY: help


build: ## Restore, build the project
	@dotnet tool restore
	@dotnet restore
	@dotnet build
.PHONY: build

test: TEST_OPTIONS ?= ""
test: ## Run tests
	@dotnet test $(TEST_OPTIONS)
.PHONY: test

build-docker: ## Build Docker images
	@docker compose build
.PHONY: build-docker

up: compose.override.yml ## Start Docker containers
	@docker compose up -d
	@docker compose ps
.PHONY: up

down: ## Stop Docker containers
	@docker compose down
.PHONY: down

ps: ## Show Docker container status
	@docker compose ps
.PHONY: ps

db: compose.override.yml ## Start DB and organisation-information-migrations services and follow organisation-information-migrations logs
	@docker compose up -d db organisation-information-migrations
	@docker compose logs -f organisation-information-migrations
.PHONY: db

OpenAPI: build ## Create OpenAPI folder and copy relevant files in
	@mkdir -p OpenAPI
	cp ./Services/CO.CDP.Tenant.WebApi/OpenAPI/CO.CDP.Tenant.WebApi.json OpenAPI/Tenant.json
	cp ./Services/CO.CDP.DataSharing.WebApi/OpenAPI/CO.CDP.DataSharing.WebApi.json OpenAPI/DataSharing.json
	cp ./Services/CO.CDP.Organisation.WebApi/OpenAPI/CO.CDP.Organisation.WebApi.json OpenAPI/Organisation.json
	cp ./Services/CO.CDP.Person.WebApi/OpenAPI/CO.CDP.Person.WebApi.json OpenAPI/Person.json
	cp ./Services/CO.CDP.Forms.WebApi/OpenAPI/CO.CDP.Forms.WebApi.json OpenAPI/Forms.json

define COMPOSE_OVERRIDE_YML
services:
  db:
    ports:
      - "$${CDP_DB_PORT:-5432}:5432"
    deploy:
      replicas: 1
  organisation-app:
    ports:
      - "$${CDP_ORGANISATION_APP_PORT:-8090}:8080"
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      OneLogin__Authority: "https://oidc.example.com"
      OneLogin__ClientId: "client-id"
      OneLogin__PrivateKey: "RSA PRIVATE KEY"
    deploy:
      replicas: 1
  tenant:
    ports:
      - "$${CDP_TENANT_PORT:-8080}:8080"
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      OneLogin__Authority: "https://oidc.example.com"
    deploy:
      replicas: 1
  organisation:
    ports:
      - '$${CDP_ORGANISATION_PORT:-8082}:8080'
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      OneLogin__Authority: "https://oidc.example.com"
    deploy:
      replicas: 1
  person:
    ports:
      - '$${CDP_PERSON_PORT:-8084}:8080'
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      OneLogin__Authority: "https://oidc.example.com"
    deploy:
      replicas: 1
  forms:
    ports:
      - '$${CDP_FORMS_PORT:-8086}:8080'
    environment:
      ASPNETCORE_ENVIRONMENT: Development
    deploy:
      replicas: 1
  data-sharing:
    ports:
      - '$${CDP_DATA_SHARING_PORT:-8088}:8080'
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      OneLogin__Discovery: "https://oidc.example.com/.well-known/openid-configuration"
    deploy:
      replicas: 1
endef

export COMPOSE_OVERRIDE_YML
compose.override.yml:
	@echo "$$COMPOSE_OVERRIDE_YML" > compose.override.yml


aws-push-to-ecr: build-docker ## Build, tag and push Docker images to ECR
	$(foreach image,$(TAGGED_IMAGES),docker tag $(image) $(REPO_URL)/$(notdir $(basename $(image)));)
	aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin $(REPO_URL)
	$(foreach image,$(IMAGES),docker push $(REPO_URL)/$(image);)
.PHONY: docker-push-to-ecr
