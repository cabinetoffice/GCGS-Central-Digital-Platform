data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_secretsmanager_secret" "teams_webhook" {
  name = "cdp-sirsi-teams-webhook-url"
}

data "aws_iam_policy_document" "notification_step_function" {
  statement {
    effect = "Allow"
    sid    = "InvokeHttpEndpoint"

    actions = [
      "states:InvokeHTTPEndpoint"
    ]
    resources = [
      aws_sfn_state_machine.teams_notification.arn
    ]
  }

  statement {
    effect = "Allow"
    sid    = "RetrieveConnectionCredentials"

    actions = [
      "events:RetrieveConnectionCredentials"
    ]
    resources = [
      aws_cloudwatch_event_connection.teams_webhook.arn
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
      aws_cloudwatch_event_connection.teams_webhook.arn,
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
      "arn:aws:secretsmanager:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:secret:events!connection/${local.name_prefix}-*",
      data.aws_secretsmanager_secret.teams_webhook.arn
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
