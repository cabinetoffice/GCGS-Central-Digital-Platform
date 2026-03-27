module "cloudfront_fts_downloads" {
  source = "../cloudfront"

  cloudfront_enabled                  = var.cloudfront_downloads_enabled
  cloudfront_log_bucket_name          = "${var.product.resource_name}-cloudfront-logs-fts-${data.aws_caller_identity.current.account_id}"
  cloudfront_manage_origin_bucket     = false
  cloudfront_name                     = "cf-fts-downloads"
  cloudfront_origin_bucket_name       = module.s3_bucket_fts.bucket
  cloudfront_realtime_log_name_suffix = "cf-rt-fts-downloads"
  cloudfront_realtime_logs_role_arn   = var.cloudfront_realtime_logs_role_arn
  cloudfront_seed_origin              = false
  environment                         = var.environment
  product                             = var.product
  tags                                = var.tags
  waf_bot_control_enabled             = false
  waf_enabled                         = false
  waf_logging_enabled                 = false
}
