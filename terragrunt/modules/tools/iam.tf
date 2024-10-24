resource "aws_iam_policy" "ecs_task_decrypt_pgadmin_secrets" {
  name   = "${local.name_prefix}-ecs-task-exec-decrypt-pgadmin-secrets"
  policy = data.aws_iam_policy_document.ecs_task_decrypt_pgadmin_secrets.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "ecs_task_decrypt_pgadmin_secrets" {
  policy_arn = aws_iam_policy.ecs_task_decrypt_pgadmin_secrets.arn
  role       = var.role_ecs_task_exec_name
}
