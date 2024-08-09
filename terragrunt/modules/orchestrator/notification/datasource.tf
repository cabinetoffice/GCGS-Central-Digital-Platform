data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_secretsmanager_secret_version" "slack_api_endpoint" {
  secret_id = "cdp-sirsi-slack-api-endpoint"
}

data "aws_iam_policy_document" "notification_step_function" {
  statement {
    effect = "Allow"
    sid = "InvokeHttpEndpoint"

    actions = [
      "states:InvokeHTTPEndpoint"
    ]

    resources = [
      aws_sfn_state_machine.slack_notification.arn
    ]
  }

  statement {
    effect = "Allow"
    sid = "RetrieveConnectionCredentials"

    actions = [
      "events:RetrieveConnectionCredentials"
    ]

    resources = [
      aws_cloudwatch_event_connection.slack.arn
    ]
  }

  statement {
    effect = "Allow"
    sid = "FetchSecretValue"

    actions = [
      "secretsmanager:GetSecretValue",
      "secretsmanager:DescribeSecret",
    ]

    resources = [
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:events!connection/${local.name_prefix}-*"
    ]
  }

  statement {
    effect = "Allow"
    sid = "FetchServiceVersionValue"

    actions = [
      "ssm:GetParameter"
    ]

    resources = [
      var.ssm_service_version_arn
    ]
  }

  statement {
    effect = "Allow"
    sid = "XRay"

    actions = [
      "xray:GetSamplingRules",
      "xray:GetSamplingTargets",
      "xray:PutTelemetryRecords",
      "xray:PutTraceSegments",
    ]

    resources = [
      aws_cloudwatch_event_connection.slack.arn
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
