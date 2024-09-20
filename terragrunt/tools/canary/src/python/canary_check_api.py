from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from aws_synthetics.selenium import synthetics_webdriver as syn_webdriver
from aws_synthetics.common import synthetics_logger as logger
from selenium.common.exceptions import StaleElementReferenceException, TimeoutException
import os
import re
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
    logger.debug(f"Attempting login to {frontend_url}")
    browser.get(frontend_url)

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
        # Updated regex to match both '0.4.0-abcdef' and '0.4.0'
        deployed_version = re.search(r"V([\d\.]+(?:-[\da-f]+)?)", version_element.text).group(1)
        if deployed_version != expected_version:
            raise Exception(
                f"Version mismatch on API landing page: expected {expected_version}, but found {deployed_version}")
        logger.info(f"Version on API landing page matches: {deployed_version}")

        take_screenshot(browser, "api_landing_page_version_matched")
    except TimeoutException as e:
        logger.error(f"Timeout while trying to find the version element on the API landing page: {e}")
        take_screenshot(browser, "api_landing_page_error")
        raise
    except AttributeError:
        logger.error("No version information found in the expected format.")
        take_screenshot(browser, "api_landing_page_version_check_failed")
        raise
    except Exception as e:
        logger.error(f"Failed to verify version on API landing page: {e}")
        take_screenshot(browser, "api_landing_page_version_check_failed")
        raise


def check_swagger_pages(browser, services, api_url, expected_version):
    logger.info("Start checking Swagger pages for each service.")

    for service in services:
        swagger_url = f"{api_url}/{service}/swagger/index.html"
        try:
            logger.debug(f"Attempting to visit Swagger page for {service}: {swagger_url}")
            browser.get(swagger_url)

            WebDriverWait(browser, 20).until(
                EC.presence_of_element_located((By.CLASS_NAME, "swagger-ui"))
            )
            logger.info(f"Swagger UI loaded successfully for {service}")

            version_element = WebDriverWait(browser, 20).until(
                EC.presence_of_element_located((By.XPATH,
                                                '/html/body/div/section/div[2]/div[2]/div[1]/section/div/div/hgroup/h2/span/small[1]/pre'))
            )
            deployed_version = version_element.text.strip()

            logger.info(f"Found version on {service} Swagger page: {deployed_version}")

            if deployed_version != expected_version:
                raise Exception(
                    f"Version mismatch on {service} Swagger page: expected {expected_version}, but found {deployed_version}")

            logger.info(f"Version on {service} Swagger page matches: {deployed_version}")

            take_screenshot(browser, f"swagger_page_{service}")

        except TimeoutException as e:
            logger.error(f"Timeout while trying to load the Swagger UI for {service}: {e}")
            take_screenshot(browser, f"swagger_page_{service}_error")
        except Exception as e:
            logger.error(f"Failed to verify version on Swagger UI for {service}: {e}")
            take_screenshot(browser, f"swagger_page_{service}_error")


def main():
    logger.debug("Fetch URL and secret name from environment variables")
    api_url = os.getenv("API_URL")
    frontend_url = api_url.replace("api.", "")
    secret_name = os.getenv("AUTH_SECRET_NAME")
    version_parameter = os.getenv("VERSION_PARAM_NAME")

    if not api_url:
        logger.error("API_URL environment variable is not set. Exiting Canary test.")
        raise Exception("API_URL environment variable is not set.")

    if not secret_name:
        logger.error("AUTH_SECRET_NAME environment variable is not set. Exiting Canary test.")
        raise Exception("AUTH_SECRET_NAME environment variable is not set.")

    if not version_parameter:
        logger.error("VERSION_PARAM_NAME environment variable is not set. Exiting Canary test.")
        raise Exception("VERSION_PARAM_NAME environment variable is not set.")

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

    services = [
        "authority",
        "data-sharing",
        "entity-verification",
        "forms",
        "organisation",
        "person",
        "tenant",
    ]

    logger.debug("Step 3: Check Swagger pages for all services")
    check_swagger_pages(browser, services, api_url, expected_version)


def handler(event, context):
    log_level = os.getenv("WEB_DRIVER_LOG_LEVEL", "WARNING")
    logger.info("Running Canary to validate frontend and API landing page. v 0.1.7")
    logger.info(f"Current Log Level is '{logger.get_level()}'")
    logger.info(f"Setting Log Level to '{log_level}'")
    logger.set_level(log_level)
    return main()
