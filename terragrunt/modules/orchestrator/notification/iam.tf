resource "aws_iam_policy" "step_function_slack_notification" {
  name   = "${local.name_prefix}-step-function-slack-notification"
  policy = data.aws_iam_policy_document.notification_step_function.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "notification_step_function" {
  policy_arn = aws_iam_policy.step_function_slack_notification.arn
  role       = var.role_notification_step_function_name
}


resource "aws_iam_policy" "orchestrator_pipeline" {
  name   = "${local.name_prefix}-orchestrator-step-function"
  policy = data.aws_iam_policy_document.orchestrator_notification.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "orchestrator_pipeline" {
  policy_arn = aws_iam_policy.orchestrator_pipeline.arn
  role       = var.role_cloudwatch_events_name
}
