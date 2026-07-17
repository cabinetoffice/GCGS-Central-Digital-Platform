resource "aws_cloudwatch_event_rule" "user_journey_monitoring" {
  count = var.environment == "development" ? 1 : 0

  name                = "${local.name_prefix}-user-journey-monitoring"
  schedule_expression = "rate(5 minutes)"
  tags                = var.tags
}

resource "aws_cloudwatch_event_target" "user_journey_monitoring" {
  count = var.environment == "development" ? 1 : 0

  rule      = aws_cloudwatch_event_rule.user_journey_monitoring[0].name
  target_id = "user-journey-monitoring"
  arn       = local.php_cluster_id
  role_arn  = var.role_cloudwatch_events_arn

  ecs_target {
    task_count          = 1
    task_definition_arn = module.ecs_service_user_journey_monitoring.task_definition_arn
    launch_type         = "FARGATE"

    network_configuration {
      assign_public_ip = false
      security_groups  = [var.ecs_sg_id]
      subnets          = var.private_subnet_ids
    }
  }

  depends_on = [
    aws_iam_role_policy_attachment.cloudwatch_event_run_user_journey_monitoring
  ]
}
