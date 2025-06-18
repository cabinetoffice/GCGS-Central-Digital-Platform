data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_ssm_parameter" "service_version_sirsi" {
  name = "${local.name_prefix}-service-version"
}

data "aws_ssm_parameter" "service_version_fts" {
  name = "${local.name_prefix}-fts-service-version"
}
