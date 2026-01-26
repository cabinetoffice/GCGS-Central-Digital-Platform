resource "aws_iam_service_linked_role" "opensearch" {
  aws_service_name = "opensearchservice.amazonaws.com"
}

resource "aws_iam_policy" "ecs_opensearch_access" {
  name   = "${local.name_prefix}-ecs-opensearch-access"
  policy = data.aws_iam_policy_document.ecs_opensearch_access.json
}

resource "aws_iam_role_policy_attachment" "opensearch_gateway_access" {
  policy_arn = aws_iam_policy.ecs_opensearch_access.arn
  role       = var.role_ecs_task_name
}

resource "aws_iam_role_policy_attachment" "opensearch_admin_access" {
  policy_arn = aws_iam_policy.ecs_opensearch_access.arn
  role       = var.role_ecs_task_opensearch_admin_name
}
