locals {
  name_prefix = var.product.resource_name
  sirsi_cluster_name = "${local.name_prefix}-cluster"
  ev_cluster_name = "${local.name_prefix}-ev-cluster"

  is_production = var.is_production || var.environment == "staging"
}
