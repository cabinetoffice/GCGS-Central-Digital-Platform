data "aws_iam_policy_document" "opensearch_admin_assume" {
  statement {
    effect = "Allow"

    principals {
      type        = "AWS"
      identifiers = [aws_iam_role.ecs_task_opensearch_admin.arn]
    }

    actions = ["sts:AssumeRole"]
  }
}

data "aws_iam_policy_document" "ecs_task_assume_opensearch_admin" {
  statement {
    actions   = ["sts:AssumeRole"]
    resources = [aws_iam_role.opensearch_admin.arn]
  }
}