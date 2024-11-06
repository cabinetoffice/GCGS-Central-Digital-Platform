resource "aws_security_group" "db_mysql" {
  description = "Security group to be attached to the MySQL DB"
  name        = "${local.name_prefix}-mysql"
  vpc_id      = var.vpc_id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-mysql"
    }
  )
}
