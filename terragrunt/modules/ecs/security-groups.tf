resource "aws_security_group_rule" "public_access_to_alb" {
  description       = "Public access to ${local.name_prefix} service via ALB"
  from_port         = 443
  protocol          = "TCP"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = var.alb_sg_id
  to_port           = 443
  type              = "ingress"
}

resource "aws_security_group_rule" "alb_to_public" {
  description       = "Public access from ${local.name_prefix} ALB, needed for Cognito authentication"
  from_port         = 443
  protocol          = "TCP"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = var.alb_sg_id
  to_port           = 443
  type              = "egress"
}

resource "aws_security_group_rule" "public_access_to_alb_http" {
  description       = "Public access to ${local.name_prefix} service via ALB port 80"
  from_port         = 80
  protocol          = "TCP"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = var.alb_sg_id
  to_port           = 80
  type              = "ingress"
}

resource "aws_security_group_rule" "temp_public_access_to_alb_http" {
  description       = "Temp public access to ${local.name_prefix} service via ALB 8080-8099"
  from_port         = 8080
  protocol          = "TCP"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = var.alb_sg_id
  to_port           = 8089
  type              = "ingress"
}

resource "aws_security_group_rule" "ecs_service_to_public_https" {
  description       = "Public access from ${local.name_prefix} service"
  from_port         = 443
  protocol          = "TCP"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = var.ecs_sg_id
  to_port           = 443
  type              = "egress"
}

resource "aws_security_group_rule" "ecs_service_to_sqs" {
  description       = "SQS access from ${local.name_prefix} service"
  from_port         = 4566
  protocol          = "TCP"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = var.ecs_sg_id
  to_port           = 4566
  type              = "egress"
}

resource "aws_security_group_rule" "postgres_from_ecs_service" {
  description              = "From ECS Service"
  from_port                = 5432
  protocol                 = "TCP"
  security_group_id        = var.db_postgres_sg_id
  source_security_group_id = var.ecs_sg_id
  to_port                  = 5432
  type                     = "ingress"
}

resource "aws_security_group_rule" "ecs_service_to_postregs" {
  description              = "To RDS"
  from_port                = 5432
  protocol                 = "TCP"
  security_group_id        = var.ecs_sg_id
  source_security_group_id = var.db_postgres_sg_id
  to_port                  = 5432
  type                     = "egress"
}

resource "aws_security_group_rule" "ecs_service_to_vpce_ecr_api" {
  description              = "To ECR API VPCE"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.ecs_sg_id
  source_security_group_id = var.vpce_ecr_api_sg_id
  to_port                  = 443
  type                     = "egress"
}

resource "aws_security_group_rule" "vpce_ecr_api_from_ecs_service" {
  description              = "From ECS Service"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.vpce_ecr_api_sg_id
  source_security_group_id = var.ecs_sg_id
  to_port                  = 443
  type                     = "ingress"
}

resource "aws_security_group_rule" "ecs_service_to_vpce_ecr_dkr" {
  description              = "To ECR Docker VPCE"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.ecs_sg_id
  source_security_group_id = var.vpce_ecr_dkr_sg_id
  to_port                  = 443
  type                     = "egress"
}

resource "aws_security_group_rule" "vpce_ecr_dkr_from_ecs_service" {
  description              = "From ECS Service"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.vpce_ecr_dkr_sg_id
  source_security_group_id = var.ecs_sg_id
  to_port                  = 443
  type                     = "ingress"
}

resource "aws_security_group_rule" "ecs_service_to_vpce_logs" {
  description              = "To Logs VPCE"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.ecs_sg_id
  source_security_group_id = var.vpce_logs_sg_id
  to_port                  = 443
  type                     = "egress"
}

resource "aws_security_group_rule" "vpce_logs_from_ecs_service" {
  description              = "From ECS Service"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.vpce_logs_sg_id
  source_security_group_id = var.ecs_sg_id
  to_port                  = 443
  type                     = "ingress"
}

resource "aws_security_group_rule" "ecs_service_to_vpce_s3" {
  description       = "To S3 VPCE"
  from_port         = 443
  protocol          = "TCP"
  security_group_id = var.ecs_sg_id
  prefix_list_ids   = [var.vpce_s3_prefix_list_id]
  to_port           = 443
  type              = "egress"
}

resource "aws_security_group_rule" "ecs_service_to_vpce_secretsmanager" {
  description              = "To Secret Manager VPCE"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.ecs_sg_id
  source_security_group_id = var.vpce_secretsmanager_sg_id
  to_port                  = 443
  type                     = "egress"
}

resource "aws_security_group_rule" "vpce_secretsmanager_from_ecs_service" {
  description              = "From ECS Service"
  from_port                = 443
  protocol                 = "TCP"
  security_group_id        = var.vpce_secretsmanager_sg_id
  source_security_group_id = var.ecs_sg_id
  to_port                  = 443
  type                     = "ingress"
}
