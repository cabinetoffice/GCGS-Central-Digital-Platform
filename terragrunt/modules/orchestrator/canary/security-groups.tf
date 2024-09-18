resource "aws_security_group_rule" "canary_sg_to_target_sgs" {
  for_each = toset(local.aws_endpoint_sgs)

  description              = "${local.canary_name} to target sg ${each.value} (egress)"
  from_port                = var.https_port
  protocol                 = "tcp"
  security_group_id        = var.canary_sg_id
  source_security_group_id = each.value
  to_port                  = var.https_port
  type                     = "egress"
}

resource "aws_security_group_rule" "target_sgs_to_canary_sg" {
  for_each = toset(local.aws_endpoint_sgs)

  description              = "${local.canary_name} from target sg ${each.value} (ingress)"
  from_port                = var.https_port
  protocol                 = "tcp"
  security_group_id        = each.value
  source_security_group_id = var.canary_sg_id
  to_port                  = var.https_port
  type                     = "ingress"
}

resource "aws_security_group_rule" "canary_to_public" {
  description       = "Public access from ${local.name_prefix} Canaries"
  from_port         = var.https_port
  protocol          = "TCP"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = var.canary_sg_id
  to_port           = var.https_port
  type              = "egress"
}

resource "aws_security_group_rule" "canary_to_service_alb_egress" {
  description              = "Allow outbound traffic from ${local.canary_name} to the VPN ALB"
  from_port                = var.https_port
  protocol                 = "tcp"
  security_group_id        = var.canary_sg_id
  source_security_group_id = var.alb_sg_id
  to_port                  = var.https_port
  type                     = "egress"
}

resource "aws_security_group_rule" "canary_to_service_alb_ingress" {
  description              = "Allow outbound traffic from ${local.canary_name} to the VPN ALB"
  from_port                = var.https_port
  protocol                 = "tcp"
  source_security_group_id = var.canary_sg_id
  security_group_id        = var.alb_sg_id
  to_port                  = var.https_port
  type                     = "ingress"
}
