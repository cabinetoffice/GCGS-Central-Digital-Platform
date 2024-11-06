output "sirsi_address" {
  value = module.rds_fts.db_address
}

output "sirsi_credentials_arn" {
  value = module.rds_fts.db_credentials_arn
}

output "sirsi_kms_arn" {
  value = module.rds_fts.db_kms_arn
}

output "sirsi_name" {
  value = module.rds_fts.db_name
}
