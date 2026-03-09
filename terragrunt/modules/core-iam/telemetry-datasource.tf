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
      "logs:GetLogEvents",
      "logs:GetQueryResults",
      "logs:StartQuery",
      "logs:StopQuery",
    ]
    resources = ["*"]
  }
}
