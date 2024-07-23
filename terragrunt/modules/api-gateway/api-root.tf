resource "aws_api_gateway_method" "root" {
  authorization = "NONE"
  http_method   = "GET"
  resource_id   = aws_api_gateway_rest_api.ecs_api.root_resource_id
  rest_api_id   = aws_api_gateway_rest_api.ecs_api.id
}

resource "aws_api_gateway_integration" "root" {
  cache_key_parameters = []
  http_method             = aws_api_gateway_method.root.http_method
  integration_http_method = "GET"
  passthrough_behavior    = "WHEN_NO_MATCH"
  resource_id             = aws_api_gateway_rest_api.ecs_api.root_resource_id
  rest_api_id             = aws_api_gateway_rest_api.ecs_api.id
  type                    = "MOCK"

  request_templates = {
    "application/json" = "{\"statusCode\": 200}"
  }

  depends_on = [
    aws_api_gateway_method.root
  ]
}

resource "aws_api_gateway_method_response" "root" {
  rest_api_id = aws_api_gateway_rest_api.ecs_api.id
  resource_id = aws_api_gateway_rest_api.ecs_api.root_resource_id
  http_method = aws_api_gateway_method.root.http_method
  status_code = "200"

  response_models = {
    "text/html" = "Empty"
  }

  depends_on = [
    aws_api_gateway_method.root
  ]
}

resource "aws_api_gateway_integration_response" "root" {
  rest_api_id = aws_api_gateway_rest_api.ecs_api.id
  resource_id = aws_api_gateway_rest_api.ecs_api.root_resource_id
  http_method = aws_api_gateway_method.root.http_method
  status_code = aws_api_gateway_method_response.root.status_code

  response_templates = {
    "text/html" = templatefile("${path.module}/templates/landing-page.html.tftpl",
      {
        api_gateway_deployment_time = formatdate("DD-MM-YYYY hh:mm:ss", timestamp())
        endpoints                   = local.endpoints
        frontend_url                = var.public_hosted_zone_fqdn
        service_version             = data.aws_ssm_parameter.orchestrator_service_version.value
      }
    )
  }

  depends_on = [
    aws_api_gateway_integration.root,
    aws_api_gateway_method_response.root
  ]
}
