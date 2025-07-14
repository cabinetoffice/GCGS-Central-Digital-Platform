data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_subnet" "first_public_subnet" {
  id = var.public_subnet_ids[0]
}

data "aws_secretsmanager_secret_version" "allowed_ips" {
  secret_id = "cdp-sirsi-waf-allowed-ip-set-tools"
}
