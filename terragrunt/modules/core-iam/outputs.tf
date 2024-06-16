output "api_gateway_cloudwatch_arn" {
  value = aws_iam_role.api_gateway_cloudwatch.arn
}

output "api_gateway_cloudwatch_name" {
  value = aws_iam_role.api_gateway_cloudwatch.name
}

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

output "ecs_task_name" {
  value = aws_iam_role.ecs_task.name
}

output "service_deployer_step_function_arn" {
  value = aws_iam_role.service_deployer_step_function.arn
}

output "service_deployer_step_function_name" {
  value = aws_iam_role.service_deployer_step_function.name
}

output "telemetry" {
  value = aws_iam_role.telemetry.arn
}

output "terraform" {
  value = aws_iam_role.terraform.arn
}
