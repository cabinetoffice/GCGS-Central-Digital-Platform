resource "aws_iam_role" "telemetry" {
  name               = "${local.name_prefix}-telemetry"
  assume_role_policy = data.aws_iam_policy_document.telemetry_assume.json

  tags = var.tags
}

resource "aws_iam_role_policy_attachment" "grafana_role_cloudwatch_read_only_access" {
  policy_arn = data.aws_iam_policy.cloudwatch_read_only_access.arn
  role       = aws_iam_role.telemetry.id
}