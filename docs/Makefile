default: build

build: build-docker
	docker run -t --rm --name cdp-docs -v ./:/docs -w /docs cabinetoffice/asciidoctor asciidoctor index.adoc -o build/cdp.html
	docker run -t --rm --name cdp-docs -v ./:/docs -w /docs cabinetoffice/asciidoctor asciidoctor-pdf index.adoc -o build/cdp.pdf
.PHONY: build

build/cdp.html: build

publish: build/cdp.html
	git config user.email 'jakub.zalas@cabinetoffice.gov.uk'
	git config user.name 'Docs Publisher'
	git checkout gh-pages
	cp build/cdp.html index.html
	git add index.html && git commit -m 'Update the documentation' && git push origin gh-pages
.PHONY: publish

build-docker:
	docker build -t cabinetoffice/asciidoctor .
.PHONY: build-docker
