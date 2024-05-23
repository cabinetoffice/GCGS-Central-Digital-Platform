output "service_name" {
  value = try(aws_ecs_service.this[0].name, "")
}

output "task_definition_arn" {
  value = aws_ecs_task_definition.this.arn
}
