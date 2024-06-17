data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

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