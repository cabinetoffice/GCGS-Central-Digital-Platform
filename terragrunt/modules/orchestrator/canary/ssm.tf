resource "aws_ssm_parameter" "expected_service_versions" {
  for_each = var.canary_configs

  description = "This parameter stores the expected deployed version of services for the ${each.key} environment. If a pinned service version is specified, it will be used. Otherwise, the latest released version (from 'cdp-sirsi-service-version') will be applied."

  insecure_value = coalesce(each.value.pinned_service_version, data.aws_ssm_parameter.cdp_sirsi_service_version.value)
  name           = "${local.name_prefix}-expected-service-version-${each.value.name}"
  tags           = var.tags
  type           = "String"
}
