resource "aws_cloudwatch_metric_alarm" "rds_cpu_high" {
  for_each = toset(var.rds_instance_ids)

  alarm_actions             = [aws_sns_topic.alerts_topic.arn]
  alarm_description         = "`${var.environment}` \n `${each.value}` \n RDS CPU Utilisation exceed `${local.rds_threshold_cup_percent}%`"
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
    DBInstanceIdentifier = each.value
  }
}


resource "aws_cloudwatch_metric_alarm" "rds_freeable_memory_low" {
  for_each = toset(var.rds_instance_ids)

  alarm_actions             = [aws_sns_topic.alerts_topic.arn]
  alarm_description         = "`${var.environment}` \n `${each.value}` \n RDS Freeable Memory below `${local.rds_threshold_freeable_memory_gb} GB`"
  alarm_name                = "${local.name_prefix}-${var.environment}-rds-freeable-memory-low-${each.key}"
  comparison_operator       = "LessThanThreshold"
  evaluation_periods        = 5
  datapoints_to_alarm       = 3
  insufficient_data_actions = [aws_sns_topic.alerts_topic.arn]
  metric_name               = "FreeableMemory"
  namespace                 = "AWS/RDS"
  ok_actions                = [aws_sns_topic.alerts_topic.arn]
  period                    = 60
  statistic                 = "Minimum"
  threshold                 = local.rds_threshold_freeable_memory_gb * 1024 * 1024 * 1024

  dimensions = {
    DBInstanceIdentifier = each.value
  }
}

resource "aws_cloudwatch_metric_alarm" "rds_connections_high" {
  for_each = toset(var.rds_instance_ids)

  alarm_actions             = [aws_sns_topic.alerts_topic.arn]
  alarm_description         = "`${var.environment}` \n `${each.value}` \n Database connections exceed `${local.rds_threshold_connections}`"
  alarm_name                = "${local.name_prefix}-${var.environment}-rds-high-connections-${each.key}"
  comparison_operator       = "GreaterThanThreshold"
  evaluation_periods        = 3
  datapoints_to_alarm       = 2
  insufficient_data_actions = [aws_sns_topic.alerts_topic.arn]
  metric_name               = "DatabaseConnections"
  namespace                 = "AWS/RDS"
  ok_actions                = [aws_sns_topic.alerts_topic.arn]
  period                    = 60
  statistic                 = "Maximum"
  threshold                 = local.rds_threshold_connections

  dimensions = {
    DBInstanceIdentifier = each.value
  }
}

resource "aws_cloudwatch_metric_alarm" "rds_replica_lag_high" {
  for_each = toset(var.rds_cluster_ids)

  alarm_actions             = [aws_sns_topic.alerts_topic.arn]
  alarm_description         = "`${var.environment}` \n `${each.value}` \n Aurora replica lag maximum above `${local.rds_threshold_replica_lag_seconds}s`"
  alarm_name                = "${local.name_prefix}-${var.environment}-rds-replica-lag-${each.key}"
  comparison_operator       = "GreaterThanThreshold"
  evaluation_periods        = 5
  datapoints_to_alarm       = 3
  insufficient_data_actions = [aws_sns_topic.alerts_topic.arn]
  metric_name               = "AuroraReplicaLagMaximum"
  namespace                 = "AWS/RDS"
  ok_actions                = [aws_sns_topic.alerts_topic.arn]
  period                    = 60
  statistic                 = "Maximum"
  threshold                 = local.rds_threshold_replica_lag_seconds

  dimensions = {
    DBClusterIdentifier = each.value
  }
}
