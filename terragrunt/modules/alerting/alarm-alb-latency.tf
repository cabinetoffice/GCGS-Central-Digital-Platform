resource "aws_cloudwatch_metric_alarm" "alb_target_response_time_p95_high" {
  for_each = var.ecs_services_target_group_arn_suffix_map

  alarm_actions       = [aws_sns_topic.alerts_topic.arn]
  alarm_description   = "`${var.environment}` \n `${each.key}` \n Target response time p95 above `${local.alb_threshold_target_response_time_p95_seconds}s`"
  alarm_name          = "${local.name_prefix}-${var.environment}-alb-p95-latency-${each.key}"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 5
  datapoints_to_alarm = 3
  metric_name         = "TargetResponseTime"
  namespace           = "AWS/ApplicationELB"
  ok_actions          = [aws_sns_topic.alerts_topic.arn]
  period              = 60
  extended_statistic  = "p95"
  threshold           = local.alb_threshold_target_response_time_p95_seconds

  dimensions = {
    LoadBalancer = var.ecs_alb_arn_suffix
    TargetGroup  = each.value
  }
}

resource "aws_cloudwatch_metric_alarm" "alb_target_response_time_p95_high_fts" {
  for_each = var.ecs_fts_services_target_group_arn_suffix_map

  alarm_actions       = [aws_sns_topic.alerts_topic.arn]
  alarm_description   = "`${var.environment}` \n `${each.key}` \n Target response time p95 above `${local.alb_threshold_target_response_time_p95_seconds}s` (FTS ALB)"
  alarm_name          = "${local.name_prefix}-${var.environment}-alb-p95-latency-fts-${each.key}"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 5
  datapoints_to_alarm = 3
  metric_name         = "TargetResponseTime"
  namespace           = "AWS/ApplicationELB"
  ok_actions          = [aws_sns_topic.alerts_topic.arn]
  period              = 60
  extended_statistic  = "p95"
  threshold           = local.alb_threshold_target_response_time_p95_seconds

  dimensions = {
    LoadBalancer = var.ecs_fts_alb_arn_suffix
    TargetGroup  = each.value
  }
}
