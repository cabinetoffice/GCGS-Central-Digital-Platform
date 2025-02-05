resource "aws_cloudwatch_log_group" "clamav_rest" {
  name              = "/ecs/${var.tools_configs.clamav_rest.name}"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_log_group" "healthcheck" {
  name              = "/ecs/healthcheck"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}


resource "aws_cloudwatch_log_group" "pgadmin" {
  name              = "/ecs/pgadmin"
  retention_in_days = var.environment == "production" ? 0 : 90
  tags              = var.tags
}

resource "aws_cloudwatch_event_rule" "tools_daily_redeploy" {
  name                = "${local.name_prefix}-tools-daily-redeploy"
  description         = "Triggers the tools ECS redeployment Step Function every day at 06:00 AM UTC"
  schedule_expression = "cron(0 6 * * ? *)"
}

resource "aws_cloudwatch_event_target" "pgadmin_redeploy_target" {
  for_each =  aws_sfn_state_machine.ecs_tools_force_deploy

  rule      = aws_cloudwatch_event_rule.tools_daily_redeploy.name
  arn       = each.value.arn
  role_arn  = var.role_cloudwatch_events_arn
}
