resource "aws_cloudwatch_metric_alarm" "redis_cpu_high" {
  for_each = toset(var.redis_cluster_node_ids)

  alarm_actions             = [aws_sns_topic.alerts_topic.arn]
  alarm_description         = "`${var.environment}` \n `${each.value}` \n Redis CPU Utilisation exceed `${local.redis_threshold_cpu_percent}%`"
  alarm_name                = "${local.name_prefix}-${var.environment}-redis-high-cpu-${each.key}"
  comparison_operator       = "GreaterThanThreshold"
  evaluation_periods        = 2
  insufficient_data_actions = [aws_sns_topic.alerts_topic.arn]
  metric_name               = "CPUUtilization"
  namespace                 = "AWS/ElastiCache"
  ok_actions                = [aws_sns_topic.alerts_topic.arn]
  period                    = 60
  statistic                 = "Average"
  threshold                 = local.redis_threshold_cpu_percent

  dimensions = {
    CacheClusterId = each.value
  }
}


resource "aws_cloudwatch_metric_alarm" "redis_memory_high" {
  for_each = toset(var.redis_cluster_node_ids)

  alarm_actions             = [aws_sns_topic.alerts_topic.arn]
  alarm_description         = "`${var.environment}` \n `${each.value}` \n Redis Memory Usage exceed `${local.redis_threshold_database_memory_percent}%`"
  alarm_name                = "${local.name_prefix}-${var.environment}-redis-high-memory-${each.key}"
  comparison_operator       = "GreaterThanThreshold"
  evaluation_periods        = 2
  insufficient_data_actions = [aws_sns_topic.alerts_topic.arn]
  metric_name               = "DatabaseMemoryUsagePercentage"
  namespace                 = "AWS/ElastiCache"
  ok_actions                = [aws_sns_topic.alerts_topic.arn]
  period                    = 60
  statistic                 = "Average"
  threshold                 = local.redis_threshold_database_memory_percent

  dimensions = {
    CacheClusterId = each.value
  }
}

resource "aws_cloudwatch_metric_alarm" "redis_evictions_high" {
  for_each = toset(var.redis_cluster_node_ids)

  alarm_actions             = [aws_sns_topic.alerts_topic.arn]
  alarm_description         = "`${var.environment}` \n `${each.value}` \n Redis evictions exceed `${local.redis_threshold_evictions}%`"
  alarm_name                = "${local.name_prefix}-${var.environment}-redis-high-evictions-${each.key}"
  comparison_operator       = "GreaterThanThreshold"
  evaluation_periods        = 2
  insufficient_data_actions = [aws_sns_topic.alerts_topic.arn]
  metric_name               = "Evictions"
  namespace                 = "AWS/ElastiCache"
  ok_actions                = [aws_sns_topic.alerts_topic.arn]
  period                    = 300
  statistic                 = "Sum"
  threshold                 = local.redis_threshold_evictions

  dimensions = {
    CacheClusterId = each.value
  }
}
