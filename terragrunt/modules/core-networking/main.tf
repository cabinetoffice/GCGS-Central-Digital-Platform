resource "aws_default_vpc" "default" {
  tags = merge(var.tags, { Name = "Default VPC" })
}

resource "aws_vpc" "this" {
  cidr_block           = var.vpc_cidr
  enable_dns_hostnames = true
  enable_dns_support   = true
  instance_tenancy     = "default"
  tags                 = local.tags
}

resource "aws_internet_gateway" "this" {
  tags   = merge(var.tags, { Name = "${var.product.resource_name}-ig" })
  vpc_id = aws_vpc.this.id
}

resource "aws_eip" "this" {
  tags = local.tags
}

resource "aws_nat_gateway" "this" {
  allocation_id = aws_eip.this.id
  subnet_id     = aws_subnet.public[0].id
  tags          = merge(var.tags, { Name = "${var.product.resource_name}-nat" })
}
