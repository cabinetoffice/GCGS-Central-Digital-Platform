resource "aws_security_group" "elasticache_redis" {
  description = "Security group to be attached to the Elasticache Redis"
  name        = "${local.name_prefix}-elasticache"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-elasticache"
    }
  )
}
