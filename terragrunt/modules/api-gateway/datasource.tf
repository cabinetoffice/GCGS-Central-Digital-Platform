data "aws_iam_policy_document" "step_function_manage_deployment" {
  statement {
    actions = ["apigateway:POST"]
    resources = [
      "arn:aws:apigateway:eu-west-2::/restapis/${aws_api_gateway_rest_api.ecs_api.id}/deployments"
    ]
    sid = "MangeECSService"
  }
}
