resource "aws_vpc" "this" {
  cidr_block           = var.externals_vpc_cidr
  enable_dns_hostnames = true
  enable_dns_support   = true
  instance_tenancy     = "default"
  tags                 = local.tags
}

resource "aws_internet_gateway" "this" {
  tags   = local.tags
  vpc_id = aws_vpc.this.id
}
