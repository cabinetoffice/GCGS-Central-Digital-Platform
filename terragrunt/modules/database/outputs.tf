output "cfs_cluster_address" {
  value = module.cluster_cfs.db_address
}

output "cfs_cluster_credentials_arn" {
  value = module.cluster_cfs.db_credentials_arn
}

output "cfs_cluster_credentials_kms_key_id" {
  value = module.cluster_cfs.db_kms_arn
}

output "cfs_cluster_name" {
  value = module.cluster_cfs.db_name
}

output "cluster_ids" {
  value = [module.cluster_entity_verification.cluster_id, module.cluster_fts.cluster_id, module.cluster_sirsi.cluster_id]
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

output "fts_cluster_address" {
  value = module.cluster_fts.db_address
}

output "fts_cluster_credentials_arn" {
  value = module.cluster_fts.db_credentials_arn
}

output "fts_cluster_credentials_kms_key_id" {
  value = module.cluster_fts.db_kms_arn
}

output "fts_cluster_name" {
  value = module.cluster_fts.db_name
}

output "import_instance_private_key_pem" {
  sensitive = true
  value     = tls_private_key.import_key.private_key_pem
}

output "import_instance_public_domain" {
  value = length(aws_route53_record.fts_db_import) > 0 ? aws_route53_record.fts_db_import[0].fqdn : null
}

output "import_instance_public_ip" {
  value = length(aws_instance.fts_db_import) > 0 ? aws_instance.fts_db_import[0].public_ip : null
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
