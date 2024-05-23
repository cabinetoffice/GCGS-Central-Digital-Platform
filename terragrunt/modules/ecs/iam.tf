resource "aws_iam_policy" "ecs_task_access_secrets" {
  name   = "${local.name_prefix}-ecs-task-exec-secrets"
  policy = data.aws_iam_policy_document.ecs_task_exec.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "ecs_task_access_secrets" {
  policy_arn = aws_iam_policy.ecs_task_access_secrets.arn
  role       = var.role_ecs_task_exec_name
}

resource "aws_iam_policy" "cloudwatch_event_invoke_deployer_step_function" {
  name        = "${local.name_prefix}-cloudwatch-start-step-function"
  description = "Policy for CloudWatch Events to invoke Step Functions"
  policy      = data.aws_iam_policy_document.cloudwatch_event_invoke_deployer_step_function.json
}

resource "aws_iam_role_policy_attachment" "cloudwatch_event_invoke_deployer_step_function_attachment" {
  policy_arn = aws_iam_policy.cloudwatch_event_invoke_deployer_step_function.arn
  role       = var.role_cloudwatch_events_name
}

resource "aws_iam_policy" "step_function_update_service" {
  name        = "${local.name_prefix}-step-function-update-service"
  description = "Policy for Step Functions to update ECS service"
  policy      = data.aws_iam_policy_document.service_deployer_step_function.json
}

resource "aws_iam_role_policy_attachment" "service_deployer_step_function" {
  policy_arn = aws_iam_policy.step_function_update_service.arn
  role       = var.role_service_deployer_step_function_name
}
