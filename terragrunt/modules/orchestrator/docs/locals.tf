locals {
  name_prefix    = var.product.resource_name
  bucket_name    = coalesce(var.bucket_name, "${local.name_prefix}-docs")
  website_domain = "${module.docs_bucket.bucket}.s3-website.${data.aws_region.current.region}.amazonaws.com"
}
