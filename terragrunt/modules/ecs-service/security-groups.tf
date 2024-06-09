resource "aws_security_group_rule" "ecs_service_from_ecs_alb" {
  count = var.host_port != null ? 1 : 0

  description              = "From ALB to ${var.name} service"
  from_port                = var.container_port
  protocol                 = "TCP"
  source_security_group_id = var.ecs_alb_sg_id
  security_group_id        = var.ecs_service_base_sg_id
  to_port                  = var.container_port
  type                     = "ingress"
}

resource "aws_security_group_rule" "ecs_alb_to_ecs_service" {
  count = var.host_port != null ? 1 : 0

  description              = "From ALB to ${var.name} service"
  from_port                = var.container_port
  protocol                 = "TCP"
  source_security_group_id = var.ecs_service_base_sg_id
  security_group_id        = var.ecs_alb_sg_id
  to_port                  = var.container_port
  type                     = "egress"
}
