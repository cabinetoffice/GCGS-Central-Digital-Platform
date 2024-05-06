resource "aws_vpc_endpoint" "s3" {

  vpc_id       = var.vpc_id
  service_name = "com.amazonaws.${data.aws_region.current.name}.s3"
  policy       = var.vpce_policy

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-s3"
    }
  )
}

resource "aws_vpc_endpoint_route_table_association" "vpc_route_table_association" {
  count = length(concat(var.private_route_table_ids, var.public_route_table_ids))

  vpc_endpoint_id = aws_vpc_endpoint.s3.id
  route_table_id  = element(concat(var.private_route_table_ids, var.public_route_table_ids), count.index)
}

resource "aws_vpc_endpoint" "ecr_api" {
  vpc_id              = var.vpc_id
  service_name        = "com.amazonaws.${data.aws_region.current.name}.ecr.api"
  subnet_ids          = var.private_subnet_ids
  vpc_endpoint_type   = "Interface"
  private_dns_enabled = true

  security_group_ids = [var.vpce_ecr_api_sg_id]

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ecr-api"
    }
  )
}

resource "aws_vpc_endpoint" "ecr_dkr" {
  vpc_id              = var.vpc_id
  service_name        = "com.amazonaws.${data.aws_region.current.name}.ecr.dkr"
  subnet_ids          = var.private_subnet_ids
  vpc_endpoint_type   = "Interface"
  private_dns_enabled = true

  security_group_ids = [var.vpce_ecr_api_sg_id]

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ecr-dkr"
    }
  )
}

resource "aws_vpc_endpoint" "logs" {
  vpc_id              = var.vpc_id
  service_name        = "com.amazonaws.${data.aws_region.current.name}.logs"
  subnet_ids          = var.private_subnet_ids
  vpc_endpoint_type   = "Interface"
  private_dns_enabled = true

  security_group_ids = [var.vpce_logs_sg_id]

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-logs"
    }
  )
}

resource "aws_vpc_endpoint" "secrets" {
  vpc_id              = var.vpc_id
  service_name        = "com.amazonaws.${data.aws_region.current.name}.secretsmanager"
  subnet_ids          = var.private_subnet_ids
  vpc_endpoint_type   = "Interface"
  private_dns_enabled = true

  security_group_ids = [var.vpce_secretsmanager_sg_id]

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-secretsmanager"
    }
  )
}
