locals {
  aws_endpoint_sgs = [] #[var.service_sg_id, var.logs_endpoint_sg_id, var.monitoring_endpoint_sg_id, var.ssm_endpoint_sg_id]
  name_prefix      = var.product.resource_name
  canary_name      = "${local.name_prefix}-canary"
}
