resource "aws_security_group" "vpce_ecr_api" {
  description = "Controls traffic to and from ECR API VPC endpoint"
  name        = "${local.name_prefix}-vpce-ecr-api"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-vpce-ecr-api"
    }
  )
}

resource "aws_security_group" "vpce_ecr_dkr" {
  description = "Controls traffic to and from ECR Docker VPC endpoint"
  name        = "${local.name_prefix}-vpce-ecr-dkr"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-vpce-ecr-dkr"
    }
  )
}


resource "aws_security_group" "vpce_logs" {
  description = "Controls traffic to and from Logs VPC endpoint"
  name        = "${local.name_prefix}-vpce-logs"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-vpce-logs"
    }
  )
}

resource "aws_security_group" "vpce_s3" {
  description = "Controls traffic to and from S3 VPC endpoint"
  name        = "${local.name_prefix}-vpce-s3"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-vpce-s3"
    }
  )
}

resource "aws_security_group" "vpce_secretsmanager" {
  description = "Controls traffic to and from Secret Manager VPC endpoint"
  name        = "${local.name_prefix}-vpce-secretsmanager"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-vpce-secretsmanager"
    }
  )
}
