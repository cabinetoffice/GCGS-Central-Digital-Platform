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

build-docker-parallel: VERSION ?= "undefined"
build-docker-parallel: ## Build Docker images in parallel
	@DOCKER_BUILDKIT_INLINE_CACHE=1 docker compose build --build-arg VERSION=$(VERSION)
.PHONY: build-docker-parallel

# Default to empty cache args locally
DOCKER_CACHE_ARGS ?=

# If running in GitHub Actions, set cache args
ifdef GITHUB_ACTIONS
DOCKER_CACHE_ARGS = \
	--cache-from=type=local,src=/tmp/.buildx-cache \
	--cache-to=type=local,dest=/tmp/.buildx-cache-new,mode=max
endif

build-docker: VERSION ?= "undefined"
build-docker: ## Build Docker images sequentially to reduce memory usage
	@DOCKER_BUILDKIT_INLINE_CACHE=1 docker compose build --parallel --memory=2g --build-arg VERSION=$(VERSION) $(DOCKER_CACHE_ARGS) db gateway
	@DOCKER_BUILDKIT_INLINE_CACHE=1 docker compose build --parallel --memory=2g --build-arg VERSION=$(VERSION) $(DOCKER_CACHE_ARGS) authority tenant
	@DOCKER_BUILDKIT_INLINE_CACHE=1 docker compose build --parallel --memory=2g --build-arg VERSION=$(VERSION) $(DOCKER_CACHE_ARGS) organisation person forms
	@DOCKER_BUILDKIT_INLINE_CACHE=1 docker compose build --parallel --memory=2g --build-arg VERSION=$(VERSION) $(DOCKER_CACHE_ARGS) data-sharing entity-verification
	@DOCKER_BUILDKIT_INLINE_CACHE=1 docker compose build --parallel --memory=2g --build-arg VERSION=$(VERSION) $(DOCKER_CACHE_ARGS) organisation-app commercial-tools-app commercial-tools-api
	@DOCKER_BUILDKIT_INLINE_CACHE=1 docker compose build --parallel --memory=2g --build-arg VERSION=$(VERSION) $(DOCKER_CACHE_ARGS) av-scanner scheduled-worker outbox-processor-organisation outbox-processor-entity-verification
	@DOCKER_BUILDKIT_INLINE_CACHE=1 docker compose build --parallel --memory=2g --build-arg VERSION=$(VERSION) $(DOCKER_CACHE_ARGS) organisation-information-migrations entity-verification-migrations commercial-tools-migrations
.PHONY: build-docker

up: render-compose-override ## Start Docker containers
	@docker compose ls
	@docker compose up -d
	@docker compose ps
	@docker network list
.PHONY: up

verify-up: render-compose-override ## Verify if all Docker containers have run. Migration containers are excluded but are checked in the pipeline when "make db" is run
	@timeout=60; \
	interval=5; \
	while [ $$timeout -gt 0 ]; do \
		if docker compose ps -a --format json \
			| jq --exit-status 'select(.Name != "co-cdp-organisation-information-migrations" and .Name != "co-cdp-entity-verification-migrations" and .Name != "co-cdp-commercial-tools-migrations") | select(.ExitCode != 0 or (.Health != "healthy" and .Health != ""))' > /dev/null; then \
			echo "Waiting for services to be healthy..."; \
			sleep $$interval; \
			timeout=$$(($$timeout - $$interval)); \
		else \
			echo "All services up"; \
			exit 0; \
		fi; \
	done; \
	echo "Services did not become healthy in time"; \
	docker compose ps -a --format json | jq --exit-status 'select(.ExitCode != 0 or (.Health != "healthy" and .Health != ""))'; \
	exit 1
.PHONY: verify-up

down: ## Destroy Docker containers
	@docker compose down
.PHONY: down

stop: ## Stop Docker containers
	@docker compose stop
.PHONY: down

ps: ## Show Docker container status
	@docker compose ps
.PHONY: ps

db: render-compose-override ## Start DB and DB migration services
	@docker compose up -d db
	@docker compose up organisation-information-migrations entity-verification-migrations commercial-tools-migrations --abort-on-container-failure
.PHONY: db

db-dump:
	@docker compose up -d db
	@docker compose exec -T db sh -c 'PGPASSWORD=$$POSTGRES_PASSWORD pg_dump -a -U $$POSTGRES_USER -d $$POSTGRES_DB'
.PHONY: db-dump

localstack: render-compose-override ## Start the localstack service for AWS services available locally
	@docker compose up -d localstack
.PHONY: localstack

redis: render-compose-override ## Start the redis service
	@docker compose up -d redis
.PHONY: redis

OpenAPI: OPENAPI_DIR?=OpenAPI
OpenAPI: build ## Create OpenAPI folder and copy relevant files in
	@mkdir -p $(OPENAPI_DIR)
	cp ./Services/CO.CDP.Tenant.WebApi/OpenAPI/CO.CDP.Tenant.WebApi.json $(OPENAPI_DIR)/Tenant.json
	cp ./Services/CO.CDP.DataSharing.WebApi/OpenAPI/CO.CDP.DataSharing.WebApi.json $(OPENAPI_DIR)/DataSharing.json
	cp ./Services/CO.CDP.Organisation.WebApi/OpenAPI/CO.CDP.Organisation.WebApi.json $(OPENAPI_DIR)/Organisation.json
	cp ./Services/CO.CDP.Person.WebApi/OpenAPI/CO.CDP.Person.WebApi.json $(OPENAPI_DIR)/Person.json
	cp ./Services/CO.CDP.Forms.WebApi/OpenAPI/CO.CDP.Forms.WebApi.json $(OPENAPI_DIR)/Forms.json
	cp ./Services/CO.CDP.EntityVerification/OpenAPI/CO.CDP.EntityVerification.json $(OPENAPI_DIR)/EntityVerification.json

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
.PHONY: aws-push-to-ecr

aws-build-and-push-ecr: build-docker aws-push-to-ecr ## Build, tag and push Docker images to ECR
.PHONY: aws-build-and-push-ecr

version-commit: COMMIT_REF ?= HEAD
version-commit: ## Determines the last commit hash
	@git rev-parse --short "$(COMMIT_REF)"
.PHONY: version-commit

last-tag: ## Determines the last created tag on the repository
	@git for-each-ref refs/tags --sort=-creatordate --format='%(refname:short)' --count=1
.PHONY: last-tag

LOCALIZATION_PATH := Services/CO.CDP.Localization
localization-export-to-csv:
	python3 $(LOCALIZATION_PATH)/scripts/export_resx_to_csv.py $(LOCALIZATION_PATH)/StaticTextResource.resx $(LOCALIZATION_PATH)/StaticTextResource.cy.resx $(LOCALIZATION_PATH)/csv-files/StaticTextResource.csv
	python3 $(LOCALIZATION_PATH)/scripts/export_resx_to_csv.py $(LOCALIZATION_PATH)/FormsEngineResource.resx $(LOCALIZATION_PATH)/FormsEngineResource.cy.resx $(LOCALIZATION_PATH)/csv-files/FormsEngineResource.csv
.PHONY: localization-export-to-csv

localization-import-from-csv:
	python3 $(LOCALIZATION_PATH)/scripts/import_csv_to_resx.py $(LOCALIZATION_PATH)/csv-files/StaticTextResource.csv $(LOCALIZATION_PATH)/scripts/template.xml $(LOCALIZATION_PATH)/StaticTextResource.resx $(LOCALIZATION_PATH)/StaticTextResource.cy.resx
	python3 $(LOCALIZATION_PATH)/scripts/import_csv_to_resx.py $(LOCALIZATION_PATH)/csv-files/FormsEngineResource.csv $(LOCALIZATION_PATH)/scripts/template.xml $(LOCALIZATION_PATH)/FormsEngineResource.resx $(LOCALIZATION_PATH)/FormsEngineResource.cy.resx
.PHONY: localization-import-from-csv

render-compose-override: ## Render compose override from template and inject secrets (WIP)
	@if [ ! -f compose.override.yml ]; then \
		envsubst < compose.override.yml.template > compose.override.yml; \
		echo "compose.override.yml created from template."; \
	else \
		echo "compose.override.yml already exists. Skipping."; \
	fi
.PHONY: render-compose-override

render-compose-override-force: ## Force overwrite compose override from template
	@cp compose.override.yml.template compose.override.yml
	@echo "compose.override.yml has been overwritten from template."
.PHONY: render-compose-override-force

e2e-test: up ## Build & run e2e tests in Docker
	docker network ls
	@cd E2ETests && docker compose up --build --abort-on-container-exit --exit-code-from e2e-tests
.PHONY: make-e2e
