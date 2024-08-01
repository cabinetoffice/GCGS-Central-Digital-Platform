output "db_address" {
  value = aws_db_instance.postgres.address
}

output "db_connection_secret_arn" {
  value = aws_secretsmanager_secret.db_connection_string.arn
}

output "db_credentials" {
  value = data.aws_secretsmanager_secret.postgres.arn
}

output "db_kms_arn" {
  value = module.kms.key_arn
}

output "db_name" {
  value = aws_db_instance.postgres.db_name
}
