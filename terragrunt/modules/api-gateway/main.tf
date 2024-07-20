resource "aws_api_gateway_account" "ecs_api" {
  cloudwatch_role_arn = var.role_api_gateway_cloudwatch_arn
}

resource "aws_api_gateway_rest_api" "ecs_api" {
  description = "API Gateway in front of the ${local.name_prefix} ECS services"
  name        = local.name_prefix
  tags        = var.tags
}

resource "aws_api_gateway_deployment" "ecs_api" {
  rest_api_id = aws_api_gateway_rest_api.ecs_api.id

  triggers = {
    redeployment = sha1(jsonencode(
      flatten([
        for service_key in keys(local.services) : [
          aws_api_gateway_integration.root,
          aws_api_gateway_resource.ecs_service[service_key].id,
          aws_api_gateway_method.ecs_service[service_key].id,
          aws_api_gateway_integration.ecs_service[service_key].id,
          aws_api_gateway_method.ecs_service_proxy[service_key].id,
          aws_api_gateway_integration.ecs_service_proxy[service_key].id
        ]
      ])
    ))
  }

  lifecycle {
    create_before_destroy = true
  }

  depends_on = [
    aws_api_gateway_rest_api.ecs_api,
    aws_api_gateway_integration.ecs_service,
    aws_api_gateway_integration.ecs_service_proxy,
    aws_api_gateway_integration.root,
    aws_api_gateway_integration_response.root,
    aws_api_gateway_method.ecs_service,
    aws_api_gateway_method.ecs_service_proxy,
    aws_api_gateway_method.root,
    aws_api_gateway_method_response.ecs_service,
    aws_api_gateway_method_response.root,
    aws_api_gateway_resource.ecs_service,
    aws_api_gateway_resource.ecs_service_proxy,
  ]
}

resource "aws_api_gateway_stage" "ecs_api" {
  deployment_id = aws_api_gateway_deployment.ecs_api.id
  rest_api_id   = aws_api_gateway_rest_api.ecs_api.id
  stage_name    = "v1"

  access_log_settings {
    destination_arn = aws_cloudwatch_log_group.ecs_api.arn
    format = jsonencode({
      requestId      = "$context.requestId"
      ip             = "$context.identity.sourceIp"
      caller         = "$context.identity.caller"
      user           = "$context.identity.user"
      requestTime    = "$context.requestTime"
      httpMethod     = "$context.httpMethod"
      resourcePath   = "$context.resourcePath"
      status         = "$context.status"
      protocol       = "$context.protocol"
      responseLength = "$context.responseLength"
    })
  }

  tags = var.tags
}

resource "aws_api_gateway_base_path_mapping" "custom" {
  domain_name = aws_api_gateway_domain_name.ecs_api.id
  stage_name  = aws_api_gateway_stage.ecs_api.stage_name
  api_id      = aws_api_gateway_rest_api.ecs_api.id
}
