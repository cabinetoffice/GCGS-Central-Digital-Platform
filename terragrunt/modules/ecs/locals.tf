locals {

  aspcore_environment = "Aws${title(var.environment)}"

  ecr_urls = {
    for task in local.tasks : task => "${local.orchestrator_account_id}.dkr.ecr.eu-west-2.amazonaws.com/cdp-${task}"
  }

  name_prefix = var.product.resource_name

  one_loging = {
    credential_locations = {
      account_url   = "${data.aws_secretsmanager_secret.one_login.arn}:AccountUrl::"
      authority   = "${data.aws_secretsmanager_secret.one_login.arn}:Authority::"
      client_id   = "${data.aws_secretsmanager_secret.one_login.arn}:ClientId::"
      private_key = "${data.aws_secretsmanager_secret.one_login.arn}:PrivateKey::"
    }
  }

  orchestrator_account_id = var.account_ids["orchestrator"]

  orchestrator_service_version = data.aws_ssm_parameter.orchestrator_service_version.value

  production_subdomain = "supplier-information"

  service_version = var.pinned_service_version == null ? data.aws_ssm_parameter.orchestrator_service_version.value : var.pinned_service_version

  migrations = ["organisation-information-migrations", "entity-verification-migrations"]

  migration_configs = {
    for name, config in var.service_configs :
    config.name => config if contains(local.migrations, config.name)
  }

  service_configs = {
    for name, config in var.service_configs :
    config.name => config if !contains(local.migrations, config.name)
  }

  tasks = [
    for name, config in var.service_configs :
    config.name
  ]

}
