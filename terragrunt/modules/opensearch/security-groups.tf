resource "aws_security_group_rule" "ingress_443_from_esc" {
  type                     = "ingress"
  security_group_id        = var.opensearch_sg_id
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  source_security_group_id = var.ecs_sg_id
  description              = "Allow TLS from ECS services"
}

resource "aws_security_group_rule" "egress_to_vpc" {
  type              = "egress"
  security_group_id = var.opensearch_sg_id
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks       = var.private_subnets_cidr_blocks
  description       = "Allow outbound traffic to VPC private subnets"
}
