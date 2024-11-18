locals {
  envs_service_version = jsonencode({
    for env, version in var.pinned_service_versions : env => coalesce(version, data.aws_ssm_parameter.cdp_sirsi_service_version.value)
  })
  name_prefix = var.product.resource_name
}
