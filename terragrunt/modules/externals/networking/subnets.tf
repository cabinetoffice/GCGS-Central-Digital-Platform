resource "aws_subnet" "private" {
  count = length(var.externals_vpc_private_subnets)

  availability_zone = element(var.vpc_azs, count.index)
  cidr_block        = var.externals_vpc_private_subnets[count.index]
  vpc_id            = aws_vpc.this.id

  tags = merge(local.tags, {
    Name = "${var.externals_product.resource_name}-private-${element(var.vpc_azs, count.index)}"
  })
}
