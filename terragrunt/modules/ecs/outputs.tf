output "certificate_arn" {
  value = aws_acm_certificate.this.arn
}

output "ecs_alb_arn_suffix" {
  value = aws_lb.ecs.arn_suffix
}

output "ecs_alb_dns_name" {
  value = aws_lb.ecs.dns_name
}

output "ecs_cluster_id" {
  value = local.main_cluster_id
}

output "ecs_cluster_name" {
  value = local.main_cluster_name
}

output "ecs_listener_arn" {
  value = local.main_ecs_listener_arn
}

output "php_ecs_cluster_id" {
  value = local.php_cluster_id
}

output "php_ecs_cluster_name" {
  value = local.php_cluster_name
}

output "php_ecs_listener_arn" {
  value = local.php_ecs_listener_arn
}

output "s3_fts_bucket" {
  value = module.s3_bucket_fts.bucket
}

output "service_configs" {
  value = local.service_configs
}

output "service_configs_php" {
  value = local.service_configs_sirsi_php_cluster
}

output "service_version_cfs" {
  value = var.pinned_service_version_sirsi
}

output "service_version_fts" {
  value = var.pinned_service_version_sirsi
}

output "service_version_global_cfs" {
  value = nonsensitive(local.orchestrator_cfs_service_version)
}

output "service_version_global_fts" {
  value = nonsensitive(local.orchestrator_fts_service_version)
}

output "service_version_global_sirsi" {
  value = nonsensitive(local.orchestrator_sirsi_service_version)
}

output "service_version_pinned" {
  value = nonsensitive(var.pinned_service_version_sirsi == null ? "not pinned, using global ${local.orchestrator_sirsi_service_version}" : var.pinned_service_version_sirsi)
}

output "services_target_group_arn_suffix_map" {
  value = {
    (module.ecs_service_authority.service_name)                            = module.ecs_service_authority.service_target_group_arn_suffix,
    (module.ecs_service_av_scanner_app.service_name)                       = module.ecs_service_av_scanner_app.service_target_group_arn_suffix,
    (module.ecs_service_entity_verification.service_name)                  = module.ecs_service_entity_verification.service_target_group_arn_suffix,
    (module.ecs_service_forms.service_name)                                = module.ecs_service_forms.service_target_group_arn_suffix,
    (module.ecs_service_organisation.service_name)                         = module.ecs_service_organisation.service_target_group_arn_suffix,
    (module.ecs_service_organisation_app.service_name)                     = module.ecs_service_organisation_app.service_target_group_arn_suffix,
    (module.ecs_service_outbox_processor_entity_verification.service_name) = module.ecs_service_outbox_processor_entity_verification.service_target_group_arn_suffix,
    (module.ecs_service_outbox_processor_organisation.service_name)        = module.ecs_service_outbox_processor_organisation.service_target_group_arn_suffix,
    (module.ecs_service_person.service_name)                               = module.ecs_service_person.service_target_group_arn_suffix,
    (module.ecs_service_tenant.service_name)                               = module.ecs_service_tenant.service_target_group_arn_suffix,
    (module.ecs_service_fts.service_name)                                  = module.ecs_service_fts.service_target_group_arn_suffix,
    (module.ecs_service_fts.service_name)                                  = module.ecs_service_fts.service_target_group_arn_suffix,
  }
}
