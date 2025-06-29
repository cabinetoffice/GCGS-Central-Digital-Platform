resource "aws_security_group" "ec2" {
  description = "Security group to be attached to EC2 instance required during FTS/CFS DB migration"
  name        = "${local.name_prefix}-ec2"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ec2"
    }
  )
}
