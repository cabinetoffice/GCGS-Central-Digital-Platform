resource "aws_subnet" "private" {
  count = length(var.vpc_private_subnets)

  availability_zone = element(var.vpc_azs, count.index)
  cidr_block        = var.vpc_private_subnets[count.index]
  vpc_id            = aws_vpc.this.id

  tags = merge(local.tags, {
    Name = "${var.product.resource_name}-private-${element(var.vpc_azs, count.index)}"
  })
}

resource "aws_subnet" "public" {
  count = length(var.vpc_public_subnets)

  availability_zone       = element(var.vpc_azs, count.index)
  cidr_block              = element(concat(var.vpc_public_subnets, [""]), count.index)
  map_public_ip_on_launch = true
  vpc_id                  = aws_vpc.this.id

  tags = merge(local.tags, {
    Name = "${var.product.resource_name}-public-${element(var.vpc_azs, count.index)}"
  })
}
