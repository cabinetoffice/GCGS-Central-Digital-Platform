resource "aws_default_route_table" "private" {
  default_route_table_id = aws_vpc.this.default_route_table_id

  tags = merge(local.tags, {
    Name = "${var.externals_product.resource_name}-default-private"
  })
}

resource "aws_route_table_association" "private" {
  count = length(var.externals_vpc_private_subnets)

  route_table_id = aws_default_route_table.private.id
  subnet_id      = element(aws_subnet.private.*.id, count.index)
}

resource "aws_route" "private_subnet_to_internet_gateway" {
  destination_cidr_block = "0.0.0.0/0"
  gateway_id             = aws_internet_gateway.this.id
  route_table_id         = aws_default_route_table.private.id
}
