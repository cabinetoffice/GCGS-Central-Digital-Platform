locals {
  name_prefix = var.product.resource_name

  orchestrator_account_id = var.account_ids["orchestrator"]

  service_environment = var.environment == "production" ? "Production" : "Development"
}
