from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from aws_synthetics.selenium import synthetics_webdriver as syn_webdriver
from aws_synthetics.common import synthetics_logger as logger
from selenium.common.exceptions import StaleElementReferenceException, TimeoutException
import os
import re
import time
import boto3
import json


def take_screenshot(browser, file_name):
    try:
        logger.debug(f"Attempting to capture Screenshot: {file_name}")
        browser.save_screenshot(f"{file_name}.png")
        logger.info(f"Screenshot saved at {file_name}.png")
    except Exception as e:
        logger.error(f"Error while saving screenshot: {e}")


def get_credentials_from_secrets(secret_name):
    client = boto3.client('secretsmanager')
    try:
        logger.debug(f"Attempting to retrieve secrets for: {secret_name}")
        secret_value = client.get_secret_value(SecretId=secret_name)
        logger.info(f"Successfully retrieved secrets for: {secret_name}")
        secret = secret_value['SecretString']
        secret_dict = json.loads(secret)
        return secret_dict['username'], secret_dict['password']
    except Exception as e:
        logger.error(f"Unable to retrieve secrets: {e}")
        raise


def get_version_from_ssm(parameter_name):
    client = boto3.client('ssm')
    try:
        logger.debug(f"Attempting to retrieve version from SSM Parameter Store: {parameter_name}")
        response = client.get_parameter(Name=parameter_name)
        version = response['Parameter']['Value']
        logger.info(f"Successfully retrieved version: {version}")
        return version
    except Exception as e:
        logger.error(f"Unable to retrieve version from SSM: {e}")
        raise


def visit_frontend_and_login(browser, frontend_url, username, password):
    browser.get(frontend_url)
    logger.debug(f"Attempting login to {frontend_url}")
    # time.sleep(2)  # Small delay to ensure page is fully loaded

    take_screenshot(browser, "frontend_cagnito_login")

    try:
        # Wait for the username field to be present in the DOM
        wait = WebDriverWait(browser, 30)
        wait.until(EC.presence_of_element_located((By.XPATH, '//*[@id="signInFormUsername"]')))

        # Insert credentials and log in using JavaScript
        browser.execute_script('document.getElementById("signInFormUsername").value = arguments[0];', username)
        browser.execute_script('document.getElementById("signInFormPassword").value = arguments[0];', password)
        logger.info("Credentials entered using JavaScript")

        browser.execute_script('document.querySelector("[type=\'submit\']").click();')
        logger.info("Form submitted using JavaScript")

        take_screenshot(browser, "frontend_logged_in")

    except TimeoutException as e:
        logger.error(f"Error during frontend loading or login: {e}")
        take_screenshot(browser, "error_frontend_loading")
        raise e


def check_api_version(browser, api_url, expected_version):
    try:
        logger.info(f"Attempting to visit API landing page: {api_url}")
        browser.get(api_url)

        logger.debug("Wait for the version element on the API page")
        version_element = WebDriverWait(browser, 20).until(
            EC.presence_of_element_located((By.TAG_NAME, "h4"))
        )
        logger.info("Version element found on API landing page.")

        logger.debug("Verify the version")
        deployed_version = re.search(r"V([\d\.]+-[\da-f]+)", version_element.text).group(1)
        if deployed_version != expected_version:
            raise Exception(
                f"Version mismatch on API landing page: expected {expected_version}, but found {deployed_version}")
        logger.info(f"Version on API landing page matches: {deployed_version}")

        take_screenshot(browser, "api_landing_page_version_matched")
    except TimeoutException as e:
        logger.error(f"Timeout while trying to find the version element on the API landing page: {e}")
        take_screenshot(browser, "api_landing_page_error")
        raise
    except Exception as e:
        logger.error(f"Failed to verify version on API landing page: {e}")
        take_screenshot(browser, "api_landing_page_version_check_failed")
        raise


def main():
    logger.debug("Fetch URL and secret name from environment variables")
    api_url = os.getenv("API_LANDING_PAGE_URL")
    frontend_url = api_url.replace("api.", "")
    secret_name = os.getenv("AUTH_SECRET_NAME")
    version_parameter = "cdp-sirsi-service-version"

    if not api_url:
        logger.error("Environment variable 'API_LANDING_PAGE_URL' is not set. Exiting Canary test.")
        raise Exception("Environment variable 'API_LANDING_PAGE_URL' is not set.")

    if not secret_name:
        logger.error("AUTH_SECRET_NAME environment variable is not set. Exiting Canary test.")
        raise Exception("AUTH_SECRET_NAME environment variable is not set.")

    logger.debug("Retrieve credentials from Secrets Manager")
    try:
        username, password = get_credentials_from_secrets(secret_name)
        logger.info(f"Retrieved credentials for {username}")
    except Exception as e:
        logger.error(f"Failed to retrieve credentials: {e}")
        raise

    logger.debug("Retrieve version from SSM Parameter Store")
    try:
        expected_version = get_version_from_ssm(version_parameter)
    except Exception as e:
        logger.error(f"Failed to retrieve version from SSM Parameter Store: {e}")
        raise

    logger.debug("Create the browser instance")
    browser = syn_webdriver.Chrome()

    logger.debug("Step 1: Log in to the frontend")
    visit_frontend_and_login(browser, frontend_url, username, password)

    logger.debug("Step 2: Check the API version")
    check_api_version(browser, api_url, expected_version)


def handler(event, context):
    logger.info("Running Canary to validate frontend and API landing page. v 0.1.0")
    return main()
