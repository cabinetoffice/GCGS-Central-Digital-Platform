output "db_address" {
  value = aws_rds_cluster.this.endpoint
}

output "db_credentials_arn" {
  value = aws_secretsmanager_secret.master_user_credential.arn
}

output "db_kms_arn" {
  value = module.kms.key_arn
}

output "db_name" {
  value = aws_rds_cluster.this.database_name
}
