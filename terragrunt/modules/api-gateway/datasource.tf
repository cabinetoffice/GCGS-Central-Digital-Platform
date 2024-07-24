data "aws_iam_policy_document" "step_function_manage_deployment" {
  statement {
    actions = ["apigateway:POST"]
    resources = [
      "arn:aws:apigateway:eu-west-2::/restapis/${aws_api_gateway_rest_api.ecs_api.id}/deployments"
    ]
    sid = "MangeECSService"
  }
}

provider "aws" {
  alias  = "orchestrator_assume_role"
  region = "eu-west-2"
  assume_role {
    role_arn = "arn:aws:iam::${local.orchestrator_account_id}:role/${local.name_prefix}-orchestrator-read-service-version"
  }
}

data "aws_ssm_parameter" "orchestrator_service_version" {
  provider = aws.orchestrator_assume_role
  name     = "/${local.name_prefix}-service-version"
}