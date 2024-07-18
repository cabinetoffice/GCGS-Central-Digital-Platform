output "api_gateway_cloudwatch_arn" {
  value = aws_iam_role.api_gateway_cloudwatch.arn
}

output "api_gateway_cloudwatch_name" {
  value = aws_iam_role.api_gateway_cloudwatch.name
}

output "ci_build_arn" {
  value = aws_iam_role.ci_build.arn
}

output "ci_build_name" {
  value = aws_iam_role.ci_build.name
}

output "ci_pipeline_arn" {
  value = aws_iam_role.ci_pipeline.arn
}

output "ci_pipeline_name" {
  value = aws_iam_role.ci_pipeline.name
}

output "cloudwatch_events_arn" {
  value = aws_iam_role.cloudwatch_events.arn
}

output "cloudwatch_events_name" {
  value = aws_iam_role.cloudwatch_events.name
}

output "db_connection_step_function_arn" {
  value = aws_iam_role.db_connection_step_function.arn
}

output "db_connection_step_function_name" {
  value = aws_iam_role.db_connection_step_function.name
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

output "telemetry_arn" {
  value = aws_iam_role.telemetry.arn
}

output "telemetry_name" {
  value = aws_iam_role.telemetry.name
}

output "terraform_arn" {
  value = aws_iam_role.terraform.arn
}

output "terraform_name" {
  value = aws_iam_role.terraform.name
}

output "tools_arn" {
  value = aws_iam_role.tools.arn
}

output "tools_name" {
  value = aws_iam_role.tools.name
}
