resource "aws_sns_topic" "ses_json_events" {
  name = "${local.logging_prefix}-json-events"
  tags = var.tags
}

resource "aws_sns_topic_policy" "ses_json_events" {
  arn    = aws_sns_topic.ses_json_events.arn
  policy = data.aws_iam_policy_document.sns_allow_ses_publish.json
}
