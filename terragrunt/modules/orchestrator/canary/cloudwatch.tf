resource "aws_cloudwatch_metric_alarm" "canary_alarm" {

  for_each = var.canary_configs
  alarm_description   = "${local.name_prefix}-ver-${substr(each.key, 0, 3)} failure in ${each.key}"
  alarm_name          = "canary/${each.key}/${local.name_prefix}-released-version-check/failed"
  comparison_operator = "LessThanThreshold"
  datapoints_to_alarm = var.datapoints_to_alarm
  evaluation_periods  = var.evaluation_periods
  metric_name         = "SuccessPercent"
  namespace           = "CloudWatchSynthetics"
  period              = var.period
  statistic           = "Average"
  tags                = var.tags
  threshold           = 100
  treat_missing_data  = "breaching"

  dimensions = {
    "CanaryName" = aws_synthetics_canary.canary[each.key].name
  }
}
