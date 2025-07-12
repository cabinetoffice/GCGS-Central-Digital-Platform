resource "aws_ssm_parameter" "service_versions_sirsi" {
  description = "This parameter stores the SIRSI service version pinned for all accounts"
  value       = local.envs_service_version_sirsi
  name        = "${local.name_prefix}-envs-service-version"
  tags        = var.tags
  type        = "String"
}

resource "aws_ssm_parameter" "service_versions_cfs" {
  description = "This parameter stores the CFS service version pinned for all accounts"
  value       = local.envs_service_version_cfs
  name        = "${local.name_prefix}-cfs-envs-service-version"
  tags        = var.tags
  type        = "String"
}

resource "aws_ssm_parameter" "service_versions_fts" {
  description = "This parameter stores the FTS service version pinned for all accounts"
  value       = local.envs_service_version_fts
  name        = "${local.name_prefix}-fts-envs-service-version"
  tags        = var.tags
  type        = "String"
}

resource "aws_ssm_parameter" "service_versions" {
  description = "This parameter stores the all service versions pinned for all accounts"
  value       = local.envs_combined_versions
  name        = "${local.name_prefix}-all-envs-service-version"
  tags        = var.tags
  type        = "String"
}
