resource "aws_iam_policy" "step_function_manage_deployment" {
  name        = "${local.name_prefix}-step-function-manage-deployment"
  description = "Policy for Step Function to handle API Gateway Deployments"
  policy      = data.aws_iam_policy_document.step_function_manage_deployment.json
}

resource "aws_iam_role_policy_attachment" "cloudwatch_event_invoke_deployer_step_function_attachment" {
  policy_arn = aws_iam_policy.step_function_manage_deployment.arn
  role       = var.role_api_gateway_deployer_step_function_name
}
