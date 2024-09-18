locals {
  aws_endpoint_sgs = [var.vpce_secretsmanager_sg_id]
  name_prefix      = var.product.resource_name
  canary_name      = "${local.name_prefix}-canary"
}
