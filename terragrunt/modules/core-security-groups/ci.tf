resource "aws_security_group" "ci" {
  description = "Security group to be attached to CI components"
  name        = "${local.name_prefix}-ci"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ci"
    }
  )
}
