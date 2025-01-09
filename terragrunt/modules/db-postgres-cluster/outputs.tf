output "db_address" {
  value = aws_rds_cluster.this.endpoint
}

output "db_master_user_secret_arn" {
  value = aws_rds_cluster.this.master_user_secret[0].secret_arn
}

output "db_master_user_secret_kms_key_id" {
  value = aws_rds_cluster.this.master_user_secret[0].kms_key_id
}

output "db_name" {
  value = aws_rds_cluster.this.database_name
}
