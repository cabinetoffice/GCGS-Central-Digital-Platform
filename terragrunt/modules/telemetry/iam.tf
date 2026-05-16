resource "aws_iam_policy" "ecs_assume_telemetry" {
  name        = "${local.name_prefix}-ecs-assume-telemetry"
  description = "To allow assuming telemetry role"
  policy      = data.aws_iam_policy_document.ecs_assume_telemetry.json
}

resource "aws_iam_role_policy_attachment" "ecs_assume_telemetry" {
  policy_arn = aws_iam_policy.ecs_assume_telemetry.arn
  role       = var.role_ecs_task_name
}

resource "aws_iam_policy" "grafana_db_kms_decrypt" {
  name        = "${local.name_prefix}-grafana-db-kms-decrypt"
  description = "Allow ECS task execution role to decrypt Grafana DB secrets"
  policy      = data.aws_iam_policy_document.grafana_db_kms_decrypt.json
  tags        = var.tags
}

resource "aws_iam_role_policy_attachment" "grafana_db_kms_decrypt" {
  policy_arn = aws_iam_policy.grafana_db_kms_decrypt.arn
  role       = var.role_ecs_task_exec_name
}
