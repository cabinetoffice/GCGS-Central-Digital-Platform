locals {
  name_prefix = var.product.resource_name

  name_inbound  = "${local.name_prefix}-${var.environment}-ev-inbound"
  name_outbound = "${local.name_prefix}-${var.environment}-ev-outbound"
}
