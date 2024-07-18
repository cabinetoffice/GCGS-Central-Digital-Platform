data "aws_iam_policy_document" "tools_assume" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type        = "AWS"
      identifiers = [aws_iam_role.ecs_task.arn]
    }
  }
}
