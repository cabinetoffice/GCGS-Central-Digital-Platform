resource "aws_security_group_rule" "ecs_service_from_ecs_alb" {
  description              = "From ALB to ECS services on Grafana port ${var.grafana_config.port}"
  from_port                = var.grafana_config.port
  protocol                 = "TCP"
  security_group_id        = var.ecs_sg_id
  source_security_group_id = var.ecs_alb_sg_id
  to_port                  = var.grafana_config.port
  type                     = "ingress"
}

resource "aws_security_group_rule" "ecs_alb_to_ecs_service" {
  description              = "From ECS services to ALB on Grafana port ${var.grafana_config.port}"
  from_port                = var.grafana_config.port
  protocol                 = "TCP"
  security_group_id        = var.ecs_alb_sg_id
  source_security_group_id = var.ecs_sg_id
  to_port                  = var.grafana_config.port
  type                     = "egress"
}
