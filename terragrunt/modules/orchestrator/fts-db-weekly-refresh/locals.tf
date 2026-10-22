locals {
  name_prefix = var.product.resource_name

  # In the orchestrator account we often include "-orchestrator" in the product resource name.
  # For S3 bucket names, this can push us over the 63 character limit, so we strip it.
  bucket_prefix = replace(local.name_prefix, "-orchestrator", "")
}
