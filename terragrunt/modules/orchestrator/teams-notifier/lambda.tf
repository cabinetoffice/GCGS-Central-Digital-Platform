data "archive_file" "teams_notifier" {
  type        = "zip"
  source_file = "${path.module}/lambda/teams_notifier.py"
  output_path = "${path.module}/lambda/teams_notifier.zip"
}

resource "aws_lambda_function" "teams_notifier" {
  function_name    = "${local.name_prefix}-teams-notifier"
  role             = aws_iam_role.teams_notifier.arn
  handler          = "teams_notifier.handler"
  runtime          = "python3.11"
  filename         = data.archive_file.teams_notifier.output_path
  source_code_hash = data.archive_file.teams_notifier.output_base64sha256
  timeout          = var.lambda_timeout
  memory_size      = var.lambda_memory

  environment {
    variables = {
      TEAMS_SECRET_ARN       = data.aws_secretsmanager_secret.teams_notifier.arn
      TEAMS_MESSAGE_TABLE    = aws_dynamodb_table.teams_notifier.name
      GRAPH_BASE             = "https://graph.microsoft.com/v1.0"
      SIRSI_VERSIONS_PARAM   = var.sirsi_versions_param_name
      FTS_VERSIONS_PARAM     = var.fts_versions_param_name
      CFS_VERSIONS_PARAM     = var.cfs_versions_param_name
      VERSIONS_CACHE_TTL_SECONDS = tostring(var.versions_cache_ttl_seconds)
    }
  }

  tags = var.tags
}

resource "aws_cloudwatch_log_group" "teams_notifier" {
  name              = "/aws/lambda/${aws_lambda_function.teams_notifier.function_name}"
  retention_in_days = 30
  tags              = var.tags
}
