output "service_name" {
  value = try(aws_ecs_service.this[0].name, "")
}


output "service_target_group_arn" {
  value = try(aws_lb_target_group.external[0].arn, "")
}

output "service_extra_target_group_arns" {
  value = { for name, tg in aws_lb_target_group.external_extra : name => tg.arn }
}

output "service_target_group_arn_suffix" {
  value = try(aws_lb_target_group.external[0].arn_suffix, "")
}

output "task_definition_arn" {
  value = aws_ecs_task_definition.this.arn
}
