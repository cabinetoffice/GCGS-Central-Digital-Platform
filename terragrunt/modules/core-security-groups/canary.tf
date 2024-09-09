resource "aws_security_group" "canary" {
  description = "Security group to be attached AWS Synthetic canaries"
  name        = "${local.name_prefix}-canary"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-canary"
    }
  )
}
