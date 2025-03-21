output "api_gateway_cloudwatch_arn" {
  value = aws_iam_role.api_gateway_cloudwatch.arn
}

output "api_gateway_cloudwatch_name" {
  value = aws_iam_role.api_gateway_cloudwatch.name
}

output "api_gateway_deployer_step_function_arn" {
  value = aws_iam_role.api_gateway_deployer_step_function.arn
}

output "api_gateway_deployer_step_function_name" {
  value = aws_iam_role.api_gateway_deployer_step_function.name
}

output "canary_arn" {
  value = aws_iam_role.canary.arn
}

output "canary_name" {
  value = aws_iam_role.canary.name
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

output "notification_step_function_arn" {
  value = aws_iam_role.notification_step_function.arn
}

output "notification_step_function_name" {
  value = aws_iam_role.notification_step_function.name
}

output "rds_backup_arn" {
  value = aws_iam_role.rds_backup.arn
}

output "rds_backup_name" {
  value = aws_iam_role.rds_backup.name
}

output "rds_cloudwatch_arn" {
  value = aws_iam_role.rds_cloudwatch_role.arn
}

output "rds_cloudwatch_name" {
  value = aws_iam_role.rds_cloudwatch_role.name
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
