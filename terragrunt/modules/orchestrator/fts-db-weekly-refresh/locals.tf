locals {
  name_prefix   = var.product.resource_name
  bucket_prefix = replace(local.name_prefix, "-orchestrator", "")
}
