resource "aws_iam_policy" "ses_logs_ingestor" {
  name   = "${local.name_prefix}-ses-logs-ingestor"
  policy = data.aws_iam_policy_document.ses_logs_ingestor.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "ses_logs_ingestor" {
  policy_arn = aws_iam_policy.ses_logs_ingestor.arn
  role       = var.role_ses_logs_ingestor_step_function_name
}

resource "aws_iam_policy" "cloudwatch_event_invoke_ses_logs_ingestor" {
  name        = "${local.name_prefix}-invoke-ses-logs-ingestor"
  description = "Policy for CloudWatch Events to invoke Step Functions"
  policy      = data.aws_iam_policy_document.cloudwatch_event_invoke_deployer_step_function.json
}

resource "aws_iam_role_policy_attachment" "cloudwatch_event_invoke_deployer_step_function_attachment" {
  policy_arn = aws_iam_policy.cloudwatch_event_invoke_ses_logs_ingestor.arn
  role       = var.role_ses_cloudwatch_events_name
}
