resource "aws_default_route_table" "default_route_table" {
  default_route_table_id = aws_vpc.this.default_route_table_id

  tags = merge(local.tags, {
    Name = "${var.product.resource_name}-default"
  })
}

resource "aws_route_table" "public" {
  count = length(var.vpc_public_subnets)

  vpc_id = aws_vpc.this.id

  tags = merge(local.tags, {
    Name = "${var.product.resource_name}-public-${element(var.vpc_azs, count.index)}"
  })
}

resource "aws_route_table_association" "public" {
  count = length(var.vpc_public_subnets)

  route_table_id = element(aws_route_table.public[*].id, count.index)
  subnet_id      = element(aws_subnet.public[*].id, count.index)
}

resource "aws_route_table" "private" {
  vpc_id = aws_vpc.this.id

  tags = merge(local.tags, {
    Name = "${var.product.resource_name}-private"
  })
}

resource "aws_route_table_association" "private" {
  count = length(var.vpc_private_subnets)

  route_table_id = aws_route_table.private.id
  subnet_id      = element(aws_subnet.private.*.id, count.index)
}

resource "aws_route" "public_subnet_to_internet_gateway" {
  count = length(var.vpc_public_subnets)

  destination_cidr_block = "0.0.0.0/0"
  gateway_id             = aws_internet_gateway.this.id
  route_table_id         = element(aws_route_table.public.*.id, count.index)
}

resource "aws_route" "private_subnet_to_nat_gateway" {
  destination_cidr_block = "0.0.0.0/0"
  nat_gateway_id         = aws_nat_gateway.this.id
  route_table_id         = aws_route_table.private.id
}
