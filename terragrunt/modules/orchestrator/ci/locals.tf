locals {
  name_prefix = var.product.resource_name

  orchestrator_account_id = var.account_ids["orchestrator"]

  trigger_update_ecs_service_cp_name = "trigger-${local.update_ecs_service_cb_name}"
  trigger_update_account_cp_name     = "trigger-${local.update_account_cb_name}"
  update_account_cb_name             = "update-account"
  update_ecs_service_cb_name         = "update-ecs-services"

  pipelines_update_ecs_services = [
    for name, id in var.account_ids : "arn:aws:iam::${id}:role/${local.name_prefix}-${name}-terraform"
  ]
}
