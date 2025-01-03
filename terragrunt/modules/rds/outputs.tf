output "db_address" {
  value = aws_db_instance.this.address
}

output "db_credentials_arn" {
  value = data.aws_secretsmanager_secret.this.arn
}

output "db_kms_arn" {
  value = module.kms.key_arn
}

output "db_name" {
  value = aws_db_instance.this.db_name
}
