locals {
  envs_service_version_sirsi = jsonencode({
    for env, version in var.pinned_service_versions_sirsi : env => coalesce(version, data.aws_ssm_parameter.service_version_sirsi.value)
  })

  envs_service_version_fts = jsonencode({
    for env, version in var.pinned_service_versions_fts : env => coalesce(version, data.aws_ssm_parameter.service_version_fts.value)
  })

  name_prefix = var.product.resource_name
}
