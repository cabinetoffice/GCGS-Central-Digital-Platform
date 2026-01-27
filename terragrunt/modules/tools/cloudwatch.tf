resource "aws_cloudwatch_log_group" "clamav_rest" {
  name              = "/ecs/${var.tools_configs.clamav_rest.name}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "cloud_beaver" {
  name              = "/ecs/${var.tools_configs.cloud_beaver.name}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "healthcheck" {
  name              = "/ecs/healthcheck"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "k6" {
  name              = "/ecs/${var.tools_configs.k6.name}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "opensearch_admin" {
  name              = "/ecs/${var.tools_configs.opensearch_admin.name}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "opensearch_gateway" {
  name              = "/ecs/${var.tools_configs.opensearch_gateway.name}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_event_rule" "tools_daily_redeploy" {
  name                = "${local.name_prefix}-tools-daily-redeploy"
  description         = "Triggers the tools ECS redeployment Step Function every day at 06:00 AM UTC"
  schedule_expression = "cron(0 6 * * ? *)"
}
