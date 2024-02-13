
build: IMAGE_VERSION ?= latest
build: build-tenant
.PHONY: build

build-tenant:
	$(MAKE) -C Tenant build
.PHONY: build-tenant
