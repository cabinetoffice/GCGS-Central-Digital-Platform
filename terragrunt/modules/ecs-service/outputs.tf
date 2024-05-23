output "service_name" {
  value = try(aws_ecs_service.this[0].name, "")
}
