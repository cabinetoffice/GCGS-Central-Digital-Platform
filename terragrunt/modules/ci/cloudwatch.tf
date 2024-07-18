resource "aws_cloudwatch_log_group" "update_account" {
  name              = "/${local.name_prefix}/build/${local.update_account_cb_name}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "update_ecs_services" {
  name              = "/${local.name_prefix}/build/${local.update_ecs_service_cb_name}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}
