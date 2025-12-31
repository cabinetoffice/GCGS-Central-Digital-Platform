data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_secretsmanager_secret_version" "slack_api_endpoint" {
  secret_id = "cdp-sirsi-slack-api-endpoint"
}

data "aws_secretsmanager_secret_version" "slack_configuration" {
  secret_id = "cdp-sirsi-slack-configuration"
}

data "aws_iam_policy_document" "notification_step_function" {
  statement {
    effect = "Allow"
    sid    = "InvokeHttpEndpoint"

    actions = [
      "states:InvokeHTTPEndpoint"
    ]
    resources = [
      aws_sfn_state_machine.slack_alert.arn,
      aws_sfn_state_machine.slack_notification.arn
    ]
  }

  statement {
    effect = "Allow"
    sid    = "DynamoDB"

    actions = [
      "dynamodb:GetItem",
      "dynamodb:PutItem",
      "dynamodb:UpdateItem"
    ]
    resources = [aws_dynamodb_table.pipeline_execution_timestamps.arn]
  }

  statement {
    effect = "Allow"
    sid    = "RetrieveConnectionCredentials"

    actions = [
      "events:RetrieveConnectionCredentials"
    ]
    resources = [
      aws_cloudwatch_event_connection.slack.arn,
      aws_cloudwatch_event_connection.slack_unified_notification.arn
    ]
  }

  statement {
    effect = "Allow"
    sid    = "FetchSecretValue"

    actions = [
      "secretsmanager:GetSecretValue",
      "secretsmanager:DescribeSecret",
    ]
    resources = [
      "arn:aws:secretsmanager:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:secret:events!connection/${local.name_prefix}-*"
    ]
  }

  statement {
    effect = "Allow"
    sid    = "FetchServiceVersionValue"

    actions = [
      "ssm:GetParameter"
    ]
    resources = [
      var.ssm_envs_cfs_service_version_arn,
      var.ssm_envs_combined_service_version_arn,
      var.ssm_envs_fts_service_version_arn,
      var.ssm_envs_sirsi_service_version_arn
    ]
  }

  statement {
    effect = "Allow"
    sid    = "XRay"

    actions = [
      "xray:GetSamplingRules",
      "xray:GetSamplingTargets",
      "xray:PutTelemetryRecords",
      "xray:PutTraceSegments",
    ]
    resources = [
      aws_cloudwatch_event_connection.slack.arn,
      aws_cloudwatch_event_connection.slack_unified_notification.arn,
    ]
  }

  statement {
    effect = "Allow"
    sid    = "ExecuteStates"

    actions = [
      "states:StartExecution"
    ]
    resources = [
      "arn:aws:states:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:stateMachine:${local.name_prefix}-*"
    ]
  }
}

data "aws_iam_policy_document" "orchestrator_notification" {

  statement {
    actions = [
      "states:StartExecution"
    ]
    effect    = "Allow"
    resources = ["*"]
  }
}
