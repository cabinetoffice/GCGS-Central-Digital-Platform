
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

define COMPOSE_OVERRIDE_YML
version: '3'
services:
  tenant:
    ports:
      - '8080:8080'
endef

export COMPOSE_OVERRIDE_YML
compose.override.yml:
	@echo "$$COMPOSE_OVERRIDE_YML" > compose.override.yml
