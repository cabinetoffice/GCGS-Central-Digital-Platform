locals {
  name_prefix        = var.product.resource_name
  slack_api_auth     = jsondecode(data.aws_secretsmanager_secret_version.slack_configuration.secret_string)["API_AUTH"]
  slack_api_endpoint = jsondecode(data.aws_secretsmanager_secret_version.slack_configuration.secret_string)["API_ENDPOINT"]
}
