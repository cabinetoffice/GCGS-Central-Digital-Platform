data "aws_iam_policy_document" "telemetry_assume" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type        = "AWS"
      identifiers = [aws_iam_role.ecs_task.arn]
    }
  }
}

data "aws_iam_policy" "cloudwatch_read_only_access" {
  arn = "arn:aws:iam::aws:policy/CloudWatchReadOnlyAccess"
}

resource "aws_iam_policy" "grafana_generic" {
  name   = "${local.name_prefix}-grafana-generic"
  policy = data.aws_iam_policy_document.grafana_generic.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "grafana_generic" {
  policy_arn = aws_iam_policy.grafana_generic.arn
  role       = aws_iam_role.telemetry.name
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
}
