resource "aws_security_group_rule" "public_egress_http" {
  description       = "To public packages via HTTP"
  from_port         = 80
  protocol          = "TCP"
  security_group_id = var.ci_sg_id
  cidr_blocks       = ["0.0.0.0/0"]
  to_port           = 80
  type              = "egress"
}

resource "aws_security_group_rule" "public_egress_https" {
  description       = "To public packages HTTPS"
  from_port         = 443
  protocol          = "TCP"
  security_group_id = var.ci_sg_id
  cidr_blocks       = ["0.0.0.0/0"]
  to_port           = 443
  type              = "egress"
}

resource "aws_security_group_rule" "ci_build_to_vpce_logs" {
  description              = "To Logs VPCE"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.ci_sg_id
  source_security_group_id = var.vpce_logs_sg_id
  to_port                  = 443
  type                     = "egress"
}

resource "aws_security_group_rule" "vpce_logs_from_ci_build" {
  description              = "From ${var.environment}-build codebuild Service"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.vpce_logs_sg_id
  source_security_group_id = var.ci_sg_id
  to_port                  = 443
  type                     = "ingress"
}

resource "aws_security_group_rule" "ci_build_to_vpce_s3" {
  description       = "To S3 VPCE"
  from_port         = 443
  protocol          = "TCP"
  security_group_id = var.ci_sg_id
  prefix_list_ids   = [var.vpce_s3_prefix_list_id]
  to_port           = 443
  type              = "egress"
}

resource "aws_security_group_rule" "ci_build_to_vpce_ecr_api" {
  description              = "To ECR API VPCE"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.ci_sg_id
  source_security_group_id = var.vpce_ecr_api_sg_id
  to_port                  = 443
  type                     = "egress"
}

resource "aws_security_group_rule" "vpce_ecr_api_from_ci_build" {
  description              = "From Codebuild"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.vpce_ecr_api_sg_id
  source_security_group_id = var.ci_sg_id
  to_port                  = 443
  type                     = "ingress"
}

resource "aws_security_group_rule" "ci_build_to_vpce_ecr_dkr" {
  description              = "To ECR Docker VPCE"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.ci_sg_id
  source_security_group_id = var.vpce_ecr_dkr_sg_id
  to_port                  = 443
  type                     = "egress"
}

resource "aws_security_group_rule" "vpce_ecr_dkr_from_ci_build" {
  description              = "From Codebuild"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.vpce_ecr_dkr_sg_id
  source_security_group_id = var.ci_sg_id
  to_port                  = 443
  type                     = "ingress"
}

resource "aws_security_group_rule" "ci_build_to_vpce_secretsmanager" {
  description              = "To Logs VPCE"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.ci_sg_id
  source_security_group_id = var.vpce_secretsmanager_sg_id
  to_port                  = 443
  type                     = "egress"
}

resource "aws_security_group_rule" "vpce_secretsmanager_from_ci_build" {
  description              = "From Codebuild"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.vpce_secretsmanager_sg_id
  source_security_group_id = var.ci_sg_id
  to_port                  = 443
  type                     = "ingress"
}
