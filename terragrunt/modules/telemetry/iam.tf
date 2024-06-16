resource "aws_iam_policy" "ecs_assume_telemetry" {
  name        = "${local.name_prefix}-ecs-assume-telemetry"
  description = "To allow assuming telemetry role"
  policy      = data.aws_iam_policy_document.ecs_assume_telemetry.json
}

resource "aws_iam_role_policy_attachment" "ecs_assume_telemetry" {
  policy_arn = aws_iam_policy.ecs_assume_telemetry.arn
  role       = var.role_ecs_task_name
}
