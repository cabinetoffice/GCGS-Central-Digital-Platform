locals {
  production_subdomain = "supplier-information"
  tags                 = merge(var.tags, { Name = var.product.resource_name })
}
