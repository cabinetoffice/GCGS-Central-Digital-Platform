# Define Docker/ECR attributes
AWS_ACCOUNT_ID=$$(aws sts get-caller-identity | jq -r '.Account')
REPO_URL := $(AWS_ACCOUNT_ID).dkr.ecr.eu-west-2.amazonaws.com
IMAGES := cdp-organisation-information-migrations cdp-data-sharing cdp-forms cdp-organisation-app cdp-organisation cdp-person cdp-tenant cdp-authority
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

down: ## Destroy Docker containers
	@docker compose down
.PHONY: down

stop: ## Stop Docker containers
	@docker compose stop
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
  gateway:
    ports:
      - "$${CDP_ORGANISATION_APP_PORT:-8090}:8090"
      - "$${CDP_AUTHORITY_PORT:-8092}:8092"
      - "$${CDP_TENANT_PORT:-8080}:8080"
      - '$${CDP_ORGANISATION_PORT:-8082}:8082'
      - '$${CDP_PERSON_PORT:-8084}:8084'
      - '$${CDP_FORMS_PORT:-8086}:8086'
      - '$${CDP_DATA_SHARING_PORT:-8088}:8088'
    environment:
#      CDP_ORGANISATION_APP_HOST: 'http://host.docker.internal:58090'
#      CDP_AUTHORITY_HOST: 'http://host.docker.internal:5050'
#      CDP_TENANT_HOST: 'http://host.docker.internal:58080'
#      CDP_ORGANISATION_HOST: 'http://host.docker.internal:58082'
#      CDP_PERSON_HOST: 'http://host.docker.internal:58084'
#      CDP_FORMS_HOST: 'http://host.docker.internal:58086'
#      CDP_DATA_SHARING_HOST: 'http://host.docker.internal:58088'
    deploy:
      replicas: 1
  db:
    ports:
      - "$${CDP_DB_PORT:-5432}:5432"
    deploy:
      replicas: 1
  organisation-app:
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      OneLogin__Authority: "https://oidc.example.com"
      OneLogin__ClientId: "client-id"
      OneLogin__PrivateKey: "RSA PRIVATE KEY"
    deploy:
      replicas: 1
  authority:
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      PublicKey: "-----BEGIN RSA PUBLIC KEY-----"
      PrivateKey: "-----BEGIN RSA PRIVATE KEY-----"
      OneLogin__Authority: "https://oidc.example.com"
    deploy:
      replicas: 1
  tenant:
    environment:
      ASPNETCORE_ENVIRONMENT: Development
    deploy:
      replicas: 1
  organisation:
    environment:
      ASPNETCORE_ENVIRONMENT: Development
    deploy:
      replicas: 1
  person:
    environment:
      ASPNETCORE_ENVIRONMENT: Development
    deploy:
      replicas: 1
  forms:
    environment:
      ASPNETCORE_ENVIRONMENT: Development
    deploy:
      replicas: 1
  data-sharing:
    environment:
      ASPNETCORE_ENVIRONMENT: Development
    deploy:
      replicas: 1
endef

export COMPOSE_OVERRIDE_YML
compose.override.yml:
	@echo "$$COMPOSE_OVERRIDE_YML" > compose.override.yml


generate-authority-keys: ## Generate authority's private and public keys and store in ./terragrunt/secrets/ folder
	openssl genpkey -algorithm RSA -out ./terragrunt/secrets/authority-private-key.pem -pkeyopt rsa_keygen_bits:2048
	openssl rsa -pubout -in ./terragrunt/secrets/authority-private-key.pem -outform PEM -out ./terragrunt/secrets/authority-public-key.pem
.PHONY: generate-authority-keys

aws-push-authority-keys: ## Push Authority's keys to the target AWS account
	@if aws secretsmanager describe-secret --secret-id cdp-sirsi-authority-keys > /dev/null 2>&1; then \
		echo "Secret exists, updating..."; \
		aws secretsmanager update-secret --secret-id cdp-sirsi-authority-keys --secret-string "$$(jq -n --arg priv "$$(cat ./terragrunt/secrets/authority-private-key.pem)" --arg pub "$$(cat ./terragrunt/secrets/authority-public-key.pem)" '{PRIVATE: $$priv, PUBLIC: $$pub}')"; \
	else \
		echo "Secret does not exist, creating..."; \
		aws secretsmanager create-secret --name cdp-sirsi-authority-keys --secret-string "$$(jq -n --arg priv "$$(cat ./terragrunt/secrets/authority-private-key.pem)" --arg pub "$$(cat ./terragrunt/secrets/authority-public-key.pem)" '{PRIVATE: $$priv, PUBLIC: $$pub}')"; \
	fi

aws-push-to-ecr: build-docker ## Build, tag and push Docker images to ECR
	$(foreach image,$(TAGGED_IMAGES),docker tag $(image) $(REPO_URL)/$(notdir $(basename $(image)));)
	aws ecr get-login-password --region eu-west-2 | docker login --username AWS --password-stdin $(REPO_URL)
	$(foreach image,$(IMAGES),docker push $(REPO_URL)/$(image);)
.PHONY: aws-push-to-ecr
