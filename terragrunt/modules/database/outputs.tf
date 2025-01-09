output "entity_verification_address" {
  value = module.rds_entity_verification.db_address
}

output "entity_verification_cluster_address" {
  value = module.cluster_entity_verification.db_address
}

output "entity_verification_cluster_credentials_arn" {
  value = module.cluster_entity_verification.db_master_user_secret_arn
}

output "entity_verification_cluster_credentials_kms_key_id" {
  value = module.cluster_entity_verification.db_master_user_secret_kms_key_id
}

output "entity_verification_cluster_name" {
  value = module.cluster_entity_verification.db_name
}

output "entity_verification_credentials_arn" {
  value = module.rds_entity_verification.db_credentials_arn
}

output "entity_verification_kms_arn" {
  value = module.rds_entity_verification.db_kms_arn
}

output "entity_verification_name" {
  value = module.rds_entity_verification.db_name
}

output "sirsi_address" {
  value = module.rds_sirsi.db_address
}

output "sirsi_cluster_address" {
  value = module.cluster_sirsi.db_address
}

output "sirsi_cluster_credentials_arn" {
  value = module.cluster_sirsi.db_master_user_secret_arn
}

output "sirsi_cluster_credentials_kms_key_id" {
  value = module.cluster_sirsi.db_master_user_secret_arn
}

output "sirsi_cluster_name" {
  value = module.cluster_sirsi.db_name
}

output "sirsi_credentials_arn" {
  value = module.rds_sirsi.db_credentials_arn
}

output "sirsi_kms_arn" {
  value = module.rds_sirsi.db_kms_arn
}

output "sirsi_name" {
  value = module.rds_sirsi.db_name
}
