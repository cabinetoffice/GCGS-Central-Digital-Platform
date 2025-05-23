resource "aws_security_group" "alb" {
  description = "ECS Load balancer"
  name        = "${local.name_prefix}-alb"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-alb"
    }
  )
}

resource "aws_security_group" "alb_tools" {
  description = "Tools Load balancer"
  name        = "${local.name_prefix}-alb-tools"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-alb-tools"
    }
  )
}
