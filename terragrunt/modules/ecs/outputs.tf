output "certificate_arn" {
  value = aws_acm_certificate.this.arn
}

output "cloudfront_cfs_downloads_domain_name" {
  value = var.cloudfront_downloads_enabled ? module.cloudfront_cfs_downloads.cloudfront_domain_name : null
}

output "cloudfront_fts_downloads_domain_name" {
  value = var.cloudfront_downloads_enabled ? module.cloudfront_fts_downloads.cloudfront_domain_name : null
}

output "cloudfront_fts_notice_render_cache_domain_name" {
  value = var.cloudfront_downloads_enabled ? module.cloudfront_fts_notice_render_cache.cloudfront_domain_name : null
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

output "ecs_internal_alb_dns_name" {
  value = aws_lb.ecs_internal.dns_name
}

output "ecs_internal_listener_arn" {
  value = local.internal_ecs_listener_arn
}

output "ecs_listener_arn" {
  value = local.main_ecs_listener_arn
}

output "fts_ecs_alb_arn_suffix" {
  value = aws_lb.ecs_fts.arn_suffix
}

output "fts_ecs_alb_dns_name" {
  value = aws_lb.ecs_fts.dns_name
}

output "fts_ecs_cluster_id" {
  value = local.fts_cluster_id
}

output "fts_ecs_cluster_name" {
  value = local.fts_cluster_name
}

output "fts_ecs_listener_arn" {
  value = local.fts_ecs_listener_arn
}

output "internal_domain" {
  value = local.internal_domain
}

output "ocds_exports_bucket" {
  value = module.s3_bucket_ocds_exports.bucket
}

output "ocds_exports_external_reader_oidc_params_secret_arn" {
  value = data.aws_secretsmanager_secret.ocds_exports_oidc_params.arn
}

output "ocds_exports_external_reader_oidc_provider_arn" {
  value = aws_iam_openid_connect_provider.ocds_exports.arn
}

output "ocds_exports_external_reader_oidc_role_arn" {
  value = aws_iam_role.ocds_exports_reader_oidc.arn
}

output "ocds_exports_external_reader_s3_uri_prefix" {
  value = "s3://${module.s3_bucket_ocds_exports.bucket}/${var.ocds_exports_prefix}"
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
  value = {
    for name, config in local.service_configs :
    name => {
      cpu           = config.cpu
      desired_count = config.desired_count
      memory        = config.memory
    }
  }
}

output "service_configs_fts" {
  value = {
    for name, config in local.service_configs_fts :
    name => {
      cpu           = config.cpu
      desired_count = config.desired_count
      memory        = config.memory
    }
  }
}

output "service_configs_php" {
  value = {
    for name, config in local.service_configs_php :
    name => {
      cpu           = config.cpu
      desired_count = config.desired_count
      memory        = config.memory
    }
  }
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
    (module.ecs_service_user_management_api.service_name)                  = module.ecs_service_user_management_api.service_target_group_arn_suffix,
    (module.ecs_service_user_management_app.service_name)                  = module.ecs_service_user_management_app.service_target_group_arn_suffix,
    (module.ecs_service_fts.service_name)                                  = module.ecs_service_fts.service_target_group_arn_suffix,
    (module.ecs_service_fts.service_name)                                  = module.ecs_service_fts.service_target_group_arn_suffix,
  }
}

output "services_target_group_arn_suffix_map_fts" {
  value = {
    (module.ecs_service_fts_app.service_name)        = module.ecs_service_fts_app.service_target_group_arn_suffix,
    (module.ecs_service_fts_search_api.service_name) = module.ecs_service_fts_search_api.service_target_group_arn_suffix,
  }
}
