resource "aws_cloudwatch_metric_alarm" "ecs_5xx_errors" {
  for_each = var.ecs_services_target_group_arn_suffix_map

  alarm_actions       = [aws_sns_topic.alerts_topic_app_5xx.arn]
  alarm_description   = "`${var.environment}` \n `${each.key}` \n 5xx error exceed `${local.ecs_threshold_5xx}`"
  alarm_name          = "${local.name_prefix}-${var.environment}-ecs-5xx-${each.key}"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 1
  metric_name         = "HTTPCode_Target_5XX_Count"
  namespace           = "AWS/ApplicationELB"
  ok_actions          = [aws_sns_topic.alerts_topic_app_5xx.arn]
  period              = 60
  statistic           = "Sum"
  threshold           = local.ecs_threshold_5xx

  dimensions = {
    LoadBalancer = var.ecs_alb_arn_suffix
    TargetGroup  = each.value
  }
}

# resource "aws_cloudwatch_metric_alarm" "ecs_4xx_errors" {
#   for_each = var.ecs_services_target_group_arn_suffix_map
#
#   alarm_actions       = [aws_sns_topic.alerts_topic_app_5xx.arn]
#   alarm_description   = "`${var.environment}` \n `${each.key}` \n 4xx error exceed `${local.ecs_threshold_4xx}`"
#   alarm_name          = "${local.name_prefix}-${var.environment}-ecs-4xx-${each.key}"
#   comparison_operator = "GreaterThanThreshold"
#   evaluation_periods  = 1
#   metric_name         = "HTTPCode_Target_4XX_Count"
#   namespace           = "AWS/ApplicationELB"
#   ok_actions          = [aws_sns_topic.alerts_topic_app_5xx.arn]
#   period              = 60
#   statistic           = "Sum"
#   threshold           = local.ecs_threshold_4xx
#
#   dimensions = {
#     LoadBalancer = var.ecs_alb_arn_suffix
#     TargetGroup  = each.value
#   }
# }


resource "aws_cloudwatch_metric_alarm" "ecs_5xx_errors_fts" {
  for_each = var.ecs_fts_services_target_group_arn_suffix_map

  alarm_actions       = [aws_sns_topic.alerts_topic_app_5xx.arn]
  alarm_description   = "`${var.environment}` \n `${each.key}` \n 5xx error exceed `${local.ecs_threshold_5xx}` (FTS ALB)"
  alarm_name          = "${local.name_prefix}-${var.environment}-ecs-5xx-fts-${each.key}"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 1
  metric_name         = "HTTPCode_Target_5XX_Count"
  namespace           = "AWS/ApplicationELB"
  ok_actions          = [aws_sns_topic.alerts_topic_app_5xx.arn]
  period              = 60
  statistic           = "Sum"
  threshold           = local.ecs_threshold_5xx

  dimensions = {
    LoadBalancer = var.ecs_fts_alb_arn_suffix
    TargetGroup  = each.value
  }
}
