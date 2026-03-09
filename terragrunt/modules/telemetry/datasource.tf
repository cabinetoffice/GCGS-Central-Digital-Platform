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
