resource "aws_iam_policy" "step_function_manage_secrets" {
  name        = "${local.name_prefix}-step-function-manage-secrets"
  description = "To allow updating DB connection string when creds rotated"
  policy      = data.aws_iam_policy_document.step_function_manage_secrets.json
}

resource "aws_iam_role_policy_attachment" "secret_deployer_step_function" {
  policy_arn = aws_iam_policy.step_function_manage_secrets.arn
  role       = var.role_db_connection_step_function_name
}

resource "aws_iam_policy" "cloudwatch_event_invoke_db_connection_string_step_function" {
  name        = "${local.name_prefix}-invoke-db-connection-string-step-function"
  description = "Policy for CloudWatch Events to invoke Step Functions"
  policy      = data.aws_iam_policy_document.cloudwatch_event_invoke_db_connection_string_step_function.json
}

resource "aws_iam_role_policy_attachment" "cloudwatch_event_invoke_db_connection_string_step_function_attachment" {
  policy_arn = aws_iam_policy.cloudwatch_event_invoke_db_connection_string_step_function.arn
  role       = var.role_cloudwatch_events_name
}
