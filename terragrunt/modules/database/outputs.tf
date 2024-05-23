output "db_connection_secret_arn" {
  value = aws_secretsmanager_secret.db_connection_string.arn
}

output "db_kms_arn" {
  value = aws_kms_key.rds.arn
}
