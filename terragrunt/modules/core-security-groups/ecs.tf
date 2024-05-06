resource "aws_security_group" "ecs" {
  description = "Security group to be attached to all services"
  name        = "${local.name_prefix}-ecs"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ecs"
    }
  )
}
