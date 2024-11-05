resource "aws_security_group_rule" "ecs_service_from_ecs_alb" {
  description              = "To ${local.name_prefix} MySQL"
  from_port         = 3306
  protocol          = "TCP"
  cidr_blocks       = var.externals_product.mysql_access_allowed_ip_ranges
  security_group_id = var.db_mysql_sg_id
  to_port           = 3306
  type              = "ingress"
}
