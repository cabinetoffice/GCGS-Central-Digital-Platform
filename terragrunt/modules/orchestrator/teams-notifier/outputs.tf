output "teams_notifier_lambda_arn" {
  value = aws_lambda_function.teams_notifier.arn
}

output "teams_notifier_lambda_name" {
  value = aws_lambda_function.teams_notifier.function_name
}

output "teams_notifier_table_name" {
  value = aws_dynamodb_table.teams_notifier.name
}

output "teams_notifier_secret_arn" {
  value = data.aws_secretsmanager_secret.teams_notifier.arn
}
