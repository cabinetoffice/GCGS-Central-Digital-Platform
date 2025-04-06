output "cluster_ids" {
  value = [module.cluster_entity_verification.cluster_id, module.cluster_sirsi.cluster_id]
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
