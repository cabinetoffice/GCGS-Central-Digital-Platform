resource "aws_iam_role" "ses_logs_ingestor_step_function" {
  name               = "${local.name_prefix}-ses-logs-ingestor-step-function"
  assume_role_policy = data.aws_iam_policy_document.ses_logs_ingestor_step_function.json

  tags = var.tags
}

resource "aws_iam_role" "ses_cloudwatch_events" {
  name               = "${local.name_prefix}-ses-cloudwatch-events"
  assume_role_policy = data.aws_iam_policy_document.events_assume.json

  tags = var.tags
}
