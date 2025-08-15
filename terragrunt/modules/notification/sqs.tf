resource "aws_sqs_queue" "ses_json_dlq" {
  name                      = "${local.logging_prefix}-json-events-dlq"
  message_retention_seconds = 1209600
  tags                      = var.tags
}

resource "aws_sqs_queue" "ses_json" {
  name                       = "${local.logging_prefix}-json-events"
  message_retention_seconds  = 1209600
  visibility_timeout_seconds = 60

  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.ses_json_dlq.arn
    maxReceiveCount     = 5
  })

  tags = var.tags
}

resource "aws_sqs_queue_policy" "ses_json" {
  queue_url = aws_sqs_queue.ses_json.id
  policy    = data.aws_iam_policy_document.sqs_allow_sns.json
}

resource "aws_sns_topic_subscription" "ses_json_sqs" {
  count = var.enable_ses_logs ? 1 : 0

  topic_arn = aws_sns_topic.ses_json_events.arn
  protocol  = "sqs"
  endpoint  = aws_sqs_queue.ses_json.arn
}
