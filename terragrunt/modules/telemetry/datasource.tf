data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

# Configure the provider to assume the role in the orchestrator account and fetch the latest service version
provider "aws" {
  alias  = "orchestrator_assume_role"
  region = "eu-west-2"
  assume_role {
    role_arn = "arn:aws:iam::${local.orchestrator_account_id}:role/${local.name_prefix}-orchestrator-read-service-version"
  }
}

data "aws_ssm_parameter" "orchestrator_sirsi_service_version" {
  provider = aws.orchestrator_assume_role
  name     = "/${local.name_prefix}-service-version"
}

data "aws_iam_policy_document" "ecs_assume_telemetry" {
  statement {
    sid    = "AllowAssumeTelemetry"
    effect = "Allow"

    actions = [
      "sts:AssumeRole"
    ]
    resources = [
      var.role_telemetry_arn
    ]
  }
}

data "aws_iam_policy_document" "grafana_db_kms_decrypt" {
  statement {
    sid    = "AllowDecryptGrafanaDbSecret"
    effect = "Allow"

    actions = [
      "kms:Decrypt",
      "kms:DescribeKey"
    ]

    resources = [
      module.grafana_db.db_kms_arn
    ]
  }
}

data "aws_iam_policy_document" "grafana_generic" {
  statement {
    sid    = "AllowTelemetryReadAccess"
    effect = "Allow"
    actions = [
      "ec2:DescribeRegions",
    ]
    resources = ["*"]
  }

  statement {
    sid    = "AllowCloudWatchLogsInsights"
    effect = "Allow"
    actions = [
      "logs:DescribeLogGroups",
      "logs:DescribeLogStreams",
      "logs:FilterLogEvents",
      "logs:GetLogEvents",
      "logs:GetQueryResults",
      "logs:StartQuery",
      "logs:StopQuery",
    ]
    resources = ["*"]
  }
}

data "aws_secretsmanager_secret" "grafana_alerting" {
  name = "${local.name_prefix}-${var.grafana_config.name}-alerting-webhook"
}
