data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_secretsmanager_secret_version" "waf_allowed_ips" {
  count     = var.waf_enabled ? 1 : 0
  secret_id = "${var.product.resource_name}-waf-allowed-ip-set"
}
