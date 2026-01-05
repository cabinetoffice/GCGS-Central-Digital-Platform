locals {
  envs_service_version_sirsi = jsonencode({
    for env, version in var.pinned_service_versions_sirsi : env =>
    coalesce(version, data.aws_ssm_parameter.service_version_sirsi.value)
  })

  envs_service_version_cfs = jsonencode({
    for env, version in var.pinned_service_versions_cfs : env =>
    coalesce(version, data.aws_ssm_parameter.service_version_cfs.value)
  })

  envs_service_version_fts = jsonencode({
    for env, version in var.pinned_service_versions_fts : env =>
    coalesce(version, data.aws_ssm_parameter.service_version_fts.value)
  })

  envs_combined_versions = jsonencode({
    for env in ["development", "staging", "integration", "production"] : env =>
    "S:${coalesce(var.pinned_service_versions_sirsi[env], data.aws_ssm_parameter.service_version_sirsi.value)} | F:${coalesce(var.pinned_service_versions_fts[env], data.aws_ssm_parameter.service_version_fts.value)} | C:${coalesce(var.pinned_service_versions_cfs[env], data.aws_ssm_parameter.service_version_cfs.value)}"
  })

  envs_service_versions = jsonencode({
    sirsi = {
      for env, version in var.pinned_service_versions_sirsi : env =>
      coalesce(version, data.aws_ssm_parameter.service_version_sirsi.value)
    }
    fts = {
      for env, version in var.pinned_service_versions_fts : env =>
      coalesce(version, data.aws_ssm_parameter.service_version_fts.value)
    }
  })



  name_prefix = var.product.resource_name
}
