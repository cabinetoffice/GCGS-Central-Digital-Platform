resource "aws_cloudwatch_metric_alarm" "ecs_service_cpu_high" {
  for_each = local.ecs_service_names

  alarm_name          = "${local.name_prefix}-ecs-high-cpu-${each.key}"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "CPUUtilization"
  namespace           = "AWS/ECS"
  period              = 60
  statistic           = "Average"
  threshold           = local.ecs_threshold_cup_percent
  alarm_description   = "${upper(each.value)}'s Service CPU Utilisation exceeds ${local.ecs_threshold_cup_percent}%"
  alarm_actions       = [aws_sns_topic.alerts_topic.arn]
  dimensions = {
    ClusterName = local.name_prefix
    ServiceName = each.value
  }
}
