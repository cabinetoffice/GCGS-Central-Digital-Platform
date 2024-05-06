output "cloudwatch_events_arn" {
  value = aws_iam_role.cloudwatch_events.arn
}

output "cloudwatch_events_name" {
  value = aws_iam_role.cloudwatch_events.name
}

output "ecs_task_arn" {
  value = aws_iam_role.ecs_task.arn
}

output "ecs_task_exec_arn" {
  value = aws_iam_role.ecs_task_exec.arn
}

output "ecs_task_exec_name" {
  value = aws_iam_role.ecs_task_exec.name
}

output "service_deployer_step_function_arn" {
  value = aws_iam_role.service_deployer_step_function.arn
}

output "service_deployer_step_function_name" {
  value = aws_iam_role.service_deployer_step_function.name
}
