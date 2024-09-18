# resource "aws_cloudwatch_metric_alarm" "canary_alarm" {
#   alarm_actions       = [var.sns_topic_arn]
#   alarm_description   = "${title(local.canary_name)} failure in ${upper(var.environment)}"
#   alarm_name          = "/${var.environment}/canary/${local.canary_name}/failed"
#   comparison_operator = "LessThanThreshold"
#   datapoints_to_alarm = var.datapoints_to_alarm
#   evaluation_periods  = var.evaluation_periods
#   metric_name         = "SuccessPercent"
#   namespace           = "CloudWatchSynthetics"
#   ok_actions          = [var.sns_topic_arn]
#   period              = var.period
#   statistic           = "Average"
#   tags                = var.tags
#   threshold           = 100
#   treat_missing_data  = "breaching"
#
#   dimensions = {
#     "CanaryName" = local.canary_name
#   }
# }
