resource "aws_iam_role" "ecs_task" {
  name               = "${local.name_prefix}-ecs-task"
  assume_role_policy = data.aws_iam_policy_document.ecs_task_assume.json

  tags = var.tags
}

resource "aws_iam_role" "ecs_task_exec" {
  name               = "${local.name_prefix}-ecs-task-exec"
  assume_role_policy = data.aws_iam_policy_document.ecs_task_assume.json

  tags = var.tags
}

resource "aws_iam_role_policy_attachment" "ecs_task_exec_generic" {
  role       = aws_iam_role.ecs_task_exec.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

resource "aws_iam_role" "service_deployer_step_function" {
  name               = "${local.name_prefix}-step-function-manage-services"
  assume_role_policy = data.aws_iam_policy_document.states_assume.json

  tags = var.tags
}

resource "aws_iam_role" "cloudwatch_events" {
  name               = "${local.name_prefix}-cloudwatch_events"
  assume_role_policy = data.aws_iam_policy_document.events_assume.json

  tags = var.tags
}
