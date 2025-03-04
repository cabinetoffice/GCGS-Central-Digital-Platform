resource "aws_cloudwatch_metric_alarm" "rds_cpu_high" {
  for_each = toset(var.rds_cluster_ids)

  alarm_actions             = [aws_sns_topic.alerts_topic.arn]
  alarm_description         = "`${var.environment}` \n `${each.value}` \n Redis CPU Utilisation exceed `${local.rds_threshold_cup_percent}%`"
  alarm_name                = "${local.name_prefix}-${var.environment}-rds-high-cpu-${each.key}"
  comparison_operator       = "GreaterThanThreshold"
  evaluation_periods        = 2
  insufficient_data_actions = [aws_sns_topic.alerts_topic.arn]
  metric_name               = "CPUUtilization"
  namespace                 = "AWS/RDS"
  ok_actions                = [aws_sns_topic.alerts_topic.arn]
  period                    = 60
  statistic                 = "Average"
  threshold                 = local.rds_threshold_cup_percent

  dimensions = {
    DBClusterIdentifier = each.value
  }
}


resource "aws_cloudwatch_metric_alarm" "rds_free_storage_low" {
  for_each = toset(var.rds_cluster_ids)

  alarm_actions             = [aws_sns_topic.alerts_topic.arn]
  alarm_description         = "`${var.environment}` \n `${each.value}` \n RDS Free Storage below `${local.rds_threshold_freeable_storage_gb} GB`"
  alarm_name                = "${local.name_prefix}-${var.environment}-rds-free-storage-low-${each.key}"
  comparison_operator       = "LessThanThreshold"
  evaluation_periods        = 2
  insufficient_data_actions = [aws_sns_topic.alerts_topic.arn]
  metric_name               = "FreeStorageSpace"
  namespace                 = "AWS/RDS"
  ok_actions                = [aws_sns_topic.alerts_topic.arn]
  period                    = 60
  statistic                 = "Average"
  threshold                 = local.rds_threshold_freeable_storage_gb

  dimensions = {
    DBClusterIdentifier = each.value
  }
}

resource "aws_cloudwatch_metric_alarm" "rds_connections_high" {
  for_each = toset(var.rds_cluster_ids)

  alarm_actions             = [aws_sns_topic.alerts_topic.arn]
  alarm_description         = "`${var.environment}` \n `${each.value}` \n Database connections exceed `${local.rds_threshold_connections}`"
  alarm_name                = "${local.name_prefix}-${var.environment}-redis-high-evictions-${each.key}"
  comparison_operator       = "GreaterThanThreshold"
  evaluation_periods        = 2
  insufficient_data_actions = [aws_sns_topic.alerts_topic.arn]
  metric_name               = "DatabaseConnections"
  namespace                 = "AWS/RDS"
  ok_actions                = [aws_sns_topic.alerts_topic.arn]
  period                    = 300
  statistic                 = "Sum"
  threshold                 = local.rds_threshold_connections

  dimensions = {
    DBClusterIdentifier = each.value
  }
}
