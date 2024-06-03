output "api_url" {
  value = aws_api_gateway_deployment.ecs_api.invoke_url
}
