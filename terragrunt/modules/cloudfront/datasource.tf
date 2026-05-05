data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_s3_bucket" "origin" {
  count  = var.cloudfront_manage_origin_bucket ? 0 : 1
  bucket = local.origin_bucket_name
}
