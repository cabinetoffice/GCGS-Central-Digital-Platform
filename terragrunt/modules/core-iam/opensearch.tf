resource "aws_iam_role" "ecs_task_opensearch_admin" {
  name               = "${local.name_prefix}-ecs-task-opensearch-admin"
  assume_role_policy = data.aws_iam_policy_document.ecs_task_assume.json

  tags = var.tags
}

resource "aws_iam_role" "ecs_task_opensearch_gateway" {
  name               = "${local.name_prefix}-ecs-task-opensearch-gateway"
  assume_role_policy = data.aws_iam_policy_document.ecs_task_assume.json

  tags = var.tags
}
