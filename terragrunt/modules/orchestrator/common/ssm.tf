resource "aws_ssm_parameter" "service_versions" {
  description = "This parameter stores the service version pinned for all accounts"
  value       = local.envs_service_version
  name        = "${local.name_prefix}-envs-service-version"
  tags        = var.tags
  type        = "String"
}
