resource "aws_cloudwatch_metric_alarm" "sqs_queue_depth" {
  for_each = toset(var.sqs_queue_names)

  alarm_actions       = [aws_sns_topic.alerts_topic.arn]
  alarm_description   = "`${var.environment}` \n `${each.value}` \n SQS Queue Depth exceed `${local.sqs_threshold_queue_depth}`"
  alarm_name          = "${local.name_prefix}-${var.environment}-sqs-queue-depth-${each.key}"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "ApproximateNumberOfMessagesVisible"
  namespace           = "AWS/SQS"
  ok_actions          = [aws_sns_topic.alerts_topic.arn]
  period              = 300
  statistic           = "Average"
  threshold           = local.sqs_threshold_queue_depth

  dimensions = {
    QueueName = each.value
  }
}


resource "aws_cloudwatch_metric_alarm" "sqs_message_age" {
  for_each = toset(var.sqs_queue_names)

  alarm_actions       = [aws_sns_topic.alerts_topic.arn]
  alarm_description   = "`${var.environment}` \n `${each.value}` \n Message Age exceeds `${local.sqs_threshold_message_age_min} minuts`"
  alarm_name          = "${local.name_prefix}-${var.environment}-sqs-message-age-${each.key}"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "ApproximateAgeOfOldestMessage"
  namespace           = "AWS/SQS"
  ok_actions          = [aws_sns_topic.alerts_topic.arn]
  period              = 300
  statistic           = "Maximum"
  threshold           = local.sqs_threshold_message_age_min * 60

  dimensions = {
    QueueName = each.value
  }
}
