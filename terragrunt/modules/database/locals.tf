locals {
  name_prefix = var.product.resource_name

  is_production = contains(["production", "integration"], var.environment)
}
