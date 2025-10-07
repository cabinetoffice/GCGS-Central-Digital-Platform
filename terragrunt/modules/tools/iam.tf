resource "aws_iam_policy" "cloudwatch_event_invoke_tools_deployer_step_function" {
  name        = "${local.name_prefix}-invoke-tools-deployer-step-function"
  description = "Policy for CloudWatch Events to invoke Step Functions"
  policy      = data.aws_iam_policy_document.cloudwatch_event_invoke_tools_deployer_step_function.json
}

resource "aws_iam_role_policy_attachment" "cloudwatch_event_invoke_tools_deployer_step_function_attachment" {
  policy_arn = aws_iam_policy.cloudwatch_event_invoke_tools_deployer_step_function.arn
  role       = var.role_cloudwatch_events_name
}

resource "aws_iam_policy" "step_function_manage_services" {
  name        = "${local.name_prefix}-step-function-manage-tools-services"
  description = "Policy for Step Functions to update ECS service"
  policy      = data.aws_iam_policy_document.step_function_manage_tools_services.json
}

resource "aws_iam_role_policy_attachment" "service_deployer_step_function" {
  policy_arn = aws_iam_policy.step_function_manage_services.arn
  role       = var.role_service_deployer_step_function_name
}
