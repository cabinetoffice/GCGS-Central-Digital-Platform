default: build

build: build-docker
	docker run -t --rm --name cdp-docs -v ./:/docs -w /docs cabinetoffice/asciidoctor asciidoctor index.adoc -o build/cdp.html
	docker run -t --rm --name cdp-docs -v ./:/docs -w /docs cabinetoffice/asciidoctor asciidoctor-pdf index.adoc -o build/cdp.pdf
.PHONY: build

build-docker:
	docker build -t cabinetoffice/asciidoctor .
.PHONY: build-docker
