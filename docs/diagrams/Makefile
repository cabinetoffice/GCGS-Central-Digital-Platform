
CDP_STRUCTURIZR_PORT ?= 9999
structurizr:
	docker run -it --rm -p $(CDP_STRUCTURIZR_PORT):8080 -v ./:/usr/local/structurizr structurizr/lite
.PHONY: structurizr
