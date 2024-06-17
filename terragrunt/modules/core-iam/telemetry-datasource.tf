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