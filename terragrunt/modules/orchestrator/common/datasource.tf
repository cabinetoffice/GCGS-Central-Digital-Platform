data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_ssm_parameter" "cdp_sirsi_service_version" {
  name = "cdp-sirsi-service-version"
}
