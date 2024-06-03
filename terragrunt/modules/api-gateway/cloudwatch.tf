resource "aws_cloudwatch_log_group" "ecs_api" {
  name              = "/${local.name_prefix}/api-gateway"
  retention_in_days = var.environment == "production" ? 0 : 90
}
