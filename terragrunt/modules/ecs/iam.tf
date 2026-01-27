resource "aws_iam_policy" "ecs_task_access_secrets" {
  name   = "${local.name_prefix}-ecs-task-exec-access-secrets"
  policy = data.aws_iam_policy_document.ecs_task_access_secrets.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "ecs_task_access_secrets" {
  policy_arn = aws_iam_policy.ecs_task_access_secrets.arn
  role       = var.role_ecs_task_exec_name
}

resource "aws_iam_policy" "ecs_task_access_queue" {
  name   = "${local.name_prefix}-ecs-task-access-queue"
  policy = data.aws_iam_policy_document.ecs_task_access_queue.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "ecs_task_access_queue" {
  policy_arn = aws_iam_policy.ecs_task_access_queue.arn
  role       = var.role_ecs_task_name
}

resource "aws_iam_policy" "ecs_task_access_elasticache" {
  name   = "${local.name_prefix}-ecs-task-access-elasticache"
  policy = data.aws_iam_policy_document.ecs_task_access_elasticache.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "ecs_task_access_elasticache" {
  policy_arn = aws_iam_policy.ecs_task_access_elasticache.arn
  role       = var.role_ecs_task_name
}

resource "aws_iam_policy" "ecs_task_access_opensearch" {
  name   = "${local.name_prefix}-ecs-task-opensearch"
  policy = data.aws_iam_policy_document.ecs_task_access_opensearch.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "ecs_task_opensearch" {
  role       = var.role_ecs_task_name
  policy_arn = aws_iam_policy.ecs_task_access_opensearch.arn
}

resource "aws_iam_policy" "ecs_task_access_ses" {
  name   = "${local.name_prefix}-ecs-task-access-ses"
  policy = data.aws_iam_policy_document.ecs_task_access_ses.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "ecs_task_access_ses" {
  policy_arn = aws_iam_policy.ecs_task_access_ses.arn
  role       = var.role_ecs_task_name
}

resource "aws_iam_policy" "ecs_task_serilog" {
  name   = "${local.name_prefix}-ecs-task-serilog"
  policy = data.aws_iam_policy_document.ecs_task_serilog.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "ecs_task_serilog" {
  policy_arn = aws_iam_policy.ecs_task_serilog.arn
  role       = var.role_ecs_task_name
}

resource "aws_iam_policy" "ecr_pull_from_orchestrator" {
  name   = "${local.name_prefix}-ecr-pull-from-orchestrator"
  policy = data.aws_iam_policy_document.ecr_pull_from_orchestrator.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "ecr_pull_from_orchestrator" {
  policy_arn = aws_iam_policy.ecr_pull_from_orchestrator.arn
  role       = var.role_ecs_task_exec_name
}

resource "aws_iam_policy" "cloudwatch_event_invoke_deployer_step_function" {
  name        = "${local.name_prefix}-invoke-deployer-step-function"
  description = "Policy for CloudWatch Events to invoke Step Functions"
  policy      = data.aws_iam_policy_document.cloudwatch_event_invoke_deployer_step_function.json
}

resource "aws_iam_role_policy_attachment" "cloudwatch_event_invoke_deployer_step_function_attachment" {
  policy_arn = aws_iam_policy.cloudwatch_event_invoke_deployer_step_function.arn
  role       = var.role_cloudwatch_events_name
}

resource "aws_iam_policy" "step_function_manage_services" {
  name        = "${local.name_prefix}-step-function-manage-services"
  description = "Policy for Step Functions to update ECS service"
  policy      = data.aws_iam_policy_document.step_function_manage_services.json
}

resource "aws_iam_role_policy_attachment" "service_deployer_step_function" {
  policy_arn = aws_iam_policy.step_function_manage_services.arn
  role       = var.role_service_deployer_step_function_name
}
