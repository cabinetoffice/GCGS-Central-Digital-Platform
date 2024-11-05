locals {
  tags = merge(var.tags, { Name = var.externals_product.resource_name })
}
