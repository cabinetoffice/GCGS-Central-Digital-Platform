# fts-app fails closed when it cannot get a verdict from its clamd sidecar (unreachable, timeout,
# clamd error): every such upload is rejected and ClamAvFileScanner logs an error containing
# "Rejecting upload (fail-closed)". Detected viruses are counted separately, below.
resource "aws_cloudwatch_log_metric_filter" "fts_clamav_scan_failed" {
  count = var.fts_clamav_enabled ? 1 : 0

  name           = "${local.name_prefix}-${var.environment}-fts-clamav-scan-failed"
  log_group_name = "/ecs/${var.service_configs.fts_app.name}"
  pattern        = "\"Rejecting upload (fail-closed)\""

  metric_transformation {
    name          = "FtsClamAvScanFailed"
    namespace     = "${local.name_prefix}/fts-app"
    value         = "1"
    default_value = "0"
  }
}

resource "aws_cloudwatch_metric_alarm" "fts_clamav_scan_failed" {
  count = var.fts_clamav_enabled ? 1 : 0

  alarm_actions       = [aws_sns_topic.alerts_topic.arn]
  alarm_description   = "`${var.environment}` \n `${var.service_configs.fts_app.name}` \n ClamAV scan could not complete, uploads rejected (fail-closed)"
  alarm_name          = "${local.name_prefix}-${var.environment}-fts-clamav-scan-failed"
  comparison_operator = "GreaterThanOrEqualToThreshold"
  evaluation_periods  = 1
  metric_name         = aws_cloudwatch_log_metric_filter.fts_clamav_scan_failed[0].metric_transformation[0].name
  namespace           = aws_cloudwatch_log_metric_filter.fts_clamav_scan_failed[0].metric_transformation[0].namespace
  ok_actions          = [aws_sns_topic.alerts_topic.arn]
  period              = 300
  statistic           = "Sum"
  threshold           = local.fts_clamav_threshold_scan_failures
  treat_missing_data  = "notBreaching"
}

# clamd returned a FOUND verdict: the upload was rejected and never stored, and ClamAvFileScanner
# logs a warning containing "ClamAV detected malware" with the file name and signature. Nothing
# needs cleaning up, but someone uploaded malware, so look at the log line to see who and what.
resource "aws_cloudwatch_log_metric_filter" "fts_clamav_malware_detected" {
  count = var.fts_clamav_enabled ? 1 : 0

  name           = "${local.name_prefix}-${var.environment}-fts-clamav-malware-detected"
  log_group_name = "/ecs/${var.service_configs.fts_app.name}"
  pattern        = "\"ClamAV detected malware\""

  metric_transformation {
    name          = "FtsClamAvMalwareDetected"
    namespace     = "${local.name_prefix}/fts-app"
    value         = "1"
    default_value = "0"
  }
}

resource "aws_cloudwatch_metric_alarm" "fts_clamav_malware_detected" {
  count = var.fts_clamav_enabled ? 1 : 0

  alarm_actions       = [aws_sns_topic.alerts_topic.arn]
  alarm_description   = "`${var.environment}` \n `${var.service_configs.fts_app.name}` \n ClamAV detected malware in an uploaded file (upload rejected, nothing stored)"
  alarm_name          = "${local.name_prefix}-${var.environment}-fts-clamav-malware-detected"
  comparison_operator = "GreaterThanOrEqualToThreshold"
  evaluation_periods  = 1
  metric_name         = aws_cloudwatch_log_metric_filter.fts_clamav_malware_detected[0].metric_transformation[0].name
  namespace           = aws_cloudwatch_log_metric_filter.fts_clamav_malware_detected[0].metric_transformation[0].namespace
  ok_actions          = [aws_sns_topic.alerts_topic.arn]
  period              = 300
  statistic           = "Sum"
  threshold           = local.fts_clamav_threshold_malware_detected
  treat_missing_data  = "notBreaching"
}
