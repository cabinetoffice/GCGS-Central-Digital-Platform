resource "aws_api_gateway_resource" "ecs_service" {
  for_each = local.services

  parent_id   = aws_api_gateway_rest_api.ecs_api.root_resource_id
  path_part   = each.value.name
  rest_api_id = aws_api_gateway_rest_api.ecs_api.id
}

resource "aws_api_gateway_method" "ecs_service" {
  for_each = local.services

  authorization = "NONE"
  http_method   = "GET"
  resource_id   = aws_api_gateway_resource.ecs_service[each.key].id
  rest_api_id   = aws_api_gateway_rest_api.ecs_api.id
}

resource "aws_api_gateway_integration" "ecs_service" {
  for_each = local.services

  http_method             = aws_api_gateway_method.ecs_service[each.key].http_method
  integration_http_method = "GET"
  passthrough_behavior    = "WHEN_NO_MATCH"
  resource_id             = aws_api_gateway_resource.ecs_service[each.key].id
  rest_api_id             = aws_api_gateway_rest_api.ecs_api.id
  type                    = "HTTP"
  uri                     = "https://${each.value.name}.${var.public_hosted_zone_fqdn}/swagger/index.html"
}

resource "aws_api_gateway_method_response" "ecs_service" {
  for_each = local.services

  http_method = aws_api_gateway_method.ecs_service[each.key].http_method
  resource_id = aws_api_gateway_resource.ecs_service[each.key].id
  rest_api_id = aws_api_gateway_rest_api.ecs_api.id
  status_code = "200"

  response_parameters = {
    "method.response.header.Content-Type" = true
  }
}

resource "aws_api_gateway_resource" "ecs_service_proxy" {
  for_each = local.services

  rest_api_id = aws_api_gateway_rest_api.ecs_api.id
  parent_id   = aws_api_gateway_resource.ecs_service[each.key].id
  path_part   = "{proxy+}"
}

resource "aws_api_gateway_method" "ecs_service_proxy" {
  for_each = local.services

  authorization = "NONE"
  http_method   = "ANY"
  resource_id   = aws_api_gateway_resource.ecs_service_proxy[each.key].id
  rest_api_id   = aws_api_gateway_rest_api.ecs_api.id

  request_parameters = {
    "method.request.path.proxy" = true
  }
}

resource "aws_api_gateway_integration" "ecs_service_proxy" {
  for_each = local.services

  http_method             = aws_api_gateway_method.ecs_service_proxy[each.key].http_method
  integration_http_method = "ANY"
  passthrough_behavior    = "WHEN_NO_MATCH"
  resource_id             = aws_api_gateway_resource.ecs_service_proxy[each.key].id
  rest_api_id             = aws_api_gateway_rest_api.ecs_api.id
  type                    = "HTTP_PROXY"
  uri                     = "https://${each.value.name}.${var.public_hosted_zone_fqdn}/{proxy}"

  request_parameters = {
    "integration.request.path.proxy" = "method.request.path.proxy"
  }

  cache_key_parameters = [
    "method.request.path.proxy",
  ]
}
