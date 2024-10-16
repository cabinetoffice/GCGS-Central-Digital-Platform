locals {
  deployment_environments = { for key, value in var.account_ids : key => value }
  name_prefix             = var.product.resource_name
}
