resource "aws_iam_role" "opensearch_admin" {
  name = "${local.name_prefix}-opensearch-admin"

  assume_role_policy = data.aws_iam_policy_document.opensearch_admin_assume.json
}

resource "aws_iam_role" "ecs_task_opensearch_admin" {
  name               = "${local.name_prefix}-ecs-task-opensearch-admin"
  assume_role_policy = data.aws_iam_policy_document.ecs_task_assume.json

  tags = var.tags
}

resource "aws_iam_policy" "ecs_task_assume_opensearch_admin" {
  name   = "${local.name_prefix}-ecs-task-assume-opensearch-admin"
  policy = data.aws_iam_policy_document.ecs_task_assume_opensearch_admin.json
}

resource "aws_iam_role_policy_attachment" "ecs_task_assume_opensearch_admin" {
  role       = aws_iam_role.ecs_task_opensearch_admin.name
  policy_arn = aws_iam_policy.ecs_task_assume_opensearch_admin.arn
}
