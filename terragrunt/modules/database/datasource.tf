data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_subnet" "first_public_subnet" {
  id = var.public_subnet_ids[0]
}

data "aws_ami" "al2_latest" {
  count = var.environment == "development" ? 1 : 0

  most_recent = true

  filter {
    name   = "name"
    values = ["ubuntu/images/hvm-ssd/ubuntu-jammy-22.04-amd64-server-*"]
  }

  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }
}

data "aws_secretsmanager_secret_version" "allowed_ips" {
  secret_id = "cdp-sirsi-waf-allowed-ip-set-tools"
}
