locals {
  tags = merge(var.tags, { Name = var.product.resource_name })
}
