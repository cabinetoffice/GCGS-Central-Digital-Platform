# Variables
BUCKET_NAME=cdp-sirsi-orchestrator-canary-891377225335
ZIP_FILE=lambda_package.zip
SCRIPT_SOURCE_DIR=src/python
SCRIPT_NAME=canary_check_api.py

# Install dependencies and build the project
.PHONY: install
install:
	poetry install --no-root

# Run tests (optional)
.PHONY: test
test:
	poetry run pytest

# Package the Lambda function and its dependencies, keeping the 'python' folder
.PHONY: package
package:
	rm -rf build
	mkdir -p build/python
	poetry export -f requirements.txt --without-hashes -o build/requirements.txt
	pip install -r build/requirements.txt -t build/python/
	cp $(SCRIPT_SOURCE_DIR)/$(SCRIPT_NAME) build/python/
	cd build && zip -r ../$(ZIP_FILE) python/

# Upload to S3
.PHONY: deploy
deploy:
	aws s3 cp $(ZIP_FILE) s3://$(BUCKET_NAME)/$(ZIP_FILE)

# Clean up build artifacts
.PHONY: clean
clean:
	rm -rf build $(ZIP_FILE)
