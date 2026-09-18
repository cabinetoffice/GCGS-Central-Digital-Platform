data "aws_iam_policy_document" "filestash_task_assume" {
  statement {
    effect = "Allow"
    actions = [
      "sts:AssumeRole",
    ]

    principals {
      type        = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }
  }
}

resource "aws_iam_role" "filestash_task" {
  name               = "${local.name_prefix}-e2e-reports-task"
  assume_role_policy = data.aws_iam_policy_document.filestash_task_assume.json
  tags               = var.tags
}
