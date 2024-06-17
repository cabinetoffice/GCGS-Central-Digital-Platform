output "api_domain_name" {
  value = aws_api_gateway_domain_name.ecs_api.domain_name
}

output "api_invoke_url" {
  value = aws_api_gateway_deployment.ecs_api.invoke_url
}
