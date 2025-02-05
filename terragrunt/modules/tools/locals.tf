locals {
  name_prefix = var.product.resource_name

  orchestrator_account_id = var.account_ids["orchestrator"]

  production_subdomain = "supplier-information"

  auto_redeploy_tools_service_configs = {
    for name, config in var.tools_configs :
    config.name => config if !contains(["grafana", "healthcheck", "clamav-rest"], config.name)
  }

  auto_redeploy_tools_tasks = [
    for name, config in local.auto_redeploy_tools_service_configs :
    config.name
  ]

}
