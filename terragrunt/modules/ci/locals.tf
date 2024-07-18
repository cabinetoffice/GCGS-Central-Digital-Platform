locals {
  name_prefix = var.product.resource_name

  orchestrator_account_id = var.account_ids["orchestrator"]

  update_account_cb_name     = "update-account"
  update_ecs_service_cb_name = "update-ecs-services"
}
