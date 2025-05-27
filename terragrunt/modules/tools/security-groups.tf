resource "aws_security_group_rule" "public_access_to_tools_alb" {
  description       = "Public access to ${local.name_prefix} service via ToolsALB"
  from_port         = 443
  protocol          = "TCP"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = var.alb_tools_sg_id
  to_port           = 443
  type              = "ingress"
}

resource "aws_security_group_rule" "tools_alb_to_public" {
  description       = "Public access from ${local.name_prefix} Tools ALB, needed for Cognito authentication"
  from_port         = 443
  protocol          = "TCP"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = var.alb_tools_sg_id
  to_port           = 443
  type              = "egress"
}

resource "aws_security_group_rule" "public_access_to_tools_alb_http" {
  description       = "Public access to ${local.name_prefix} service via Tools ALB port 80"
  from_port         = 80
  protocol          = "TCP"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = var.alb_tools_sg_id
  to_port           = 80
  type              = "ingress"
}

resource "aws_security_group_rule" "efs_from_ecs" {
  type                     = "ingress"
  from_port                = 2049
  to_port                  = 2049
  protocol                 = "tcp"
  source_security_group_id = var.ecs_sg_id
  security_group_id        = var.efs_sg_id
}

resource "aws_security_group_rule" "ecs_to_efs" {
  type                     = "egress"
  from_port                = 2049
  to_port                  = 2049
  protocol                 = "tcp"
  source_security_group_id = var.efs_sg_id
  security_group_id        = var.ecs_sg_id
}
