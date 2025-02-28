resource "aws_cloudwatch_metric_alarm" "ecs_service_cpu_high" {
  for_each = local.ecs_service_names

  alarm_actions             = [aws_sns_topic.alerts_topic.arn]
  alarm_description         = "`${var.environment}` \n `${each.value}` \n CPU Utilisation exceed `${local.ecs_threshold_cup_percent}%`"
  alarm_name                = "${local.name_prefix}-${var.environment}-high-cpu-${each.value}"
  comparison_operator       = "GreaterThanThreshold"
  evaluation_periods        = 2
  insufficient_data_actions = [aws_sns_topic.alerts_topic.arn]
  metric_name               = "CPUUtilization"
  namespace                 = "AWS/ECS"
  ok_actions                = [aws_sns_topic.alerts_topic.arn]
  period                    = 60
  statistic                 = "Average"
  threshold                 = local.ecs_threshold_cup_percent

  dimensions = {
    ClusterName = local.name_prefix
    ServiceName = each.value
  }
}

resource "aws_cloudwatch_metric_alarm" "ecs_service_memory_high" {
  for_each = local.ecs_service_names

  alarm_actions             = [aws_sns_topic.alerts_topic.arn]
  alarm_description         = "`${var.environment}` \n `${each.value}` \n Memory Utilisation exceed `${local.ecs_threshold_memory_percent}%`"
  alarm_name                = "${local.name_prefix}-${var.environment}-high-memory-${each.value}"
  comparison_operator       = "GreaterThanThreshold"
  evaluation_periods        = 2
  insufficient_data_actions = [aws_sns_topic.alerts_topic.arn]
  metric_name               = "MemoryUtilization"
  namespace                 = "AWS/ECS"
  ok_actions                = [aws_sns_topic.alerts_topic.arn]
  period                    = 60
  statistic                 = "Average"
  threshold                 = local.ecs_threshold_memory_percent

  dimensions = {
    ClusterName = local.name_prefix
    ServiceName = each.value
  }
}
