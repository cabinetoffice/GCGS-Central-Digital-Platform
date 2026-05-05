locals {
  name_prefix = var.product.resource_name
  cf_name_prefix = "${local.name_prefix}-${var.cloudfront_name}"

  origin_bucket_name = var.cloudfront_origin_bucket_name != null ? var.cloudfront_origin_bucket_name : "${local.name_prefix}-cloudfront-origin-${data.aws_caller_identity.current.account_id}"
  log_bucket_name    = var.cloudfront_log_bucket_name != null ? var.cloudfront_log_bucket_name : "${local.name_prefix}-cloudfront-logs-${data.aws_caller_identity.current.account_id}"

  default_origin_id = "s3-origin"

  origin_bucket_domain_name = var.cloudfront_manage_origin_bucket ? aws_s3_bucket.origin[0].bucket_regional_domain_name : data.aws_s3_bucket.origin[0].bucket_regional_domain_name
  origin_bucket_arn         = var.cloudfront_manage_origin_bucket ? aws_s3_bucket.origin[0].arn : data.aws_s3_bucket.origin[0].arn
  origin_bucket_id          = var.cloudfront_manage_origin_bucket ? aws_s3_bucket.origin[0].id : data.aws_s3_bucket.origin[0].id

  ordered_cache_behaviors = []

  waf_log_group_name = "aws-waf-logs-${local.cf_name_prefix}"

  normalized_aliases = [for alias in var.cloudfront_aliases : trimsuffix(alias, ".")]
  primary_alias      = length(local.normalized_aliases) > 0 ? local.normalized_aliases[0] : null

  acm_certificate_arn = var.cloudfront_acm_certificate_arn != null ? var.cloudfront_acm_certificate_arn : try(aws_acm_certificate.cloudfront[0].arn, null)

  waf_rule_sets_priority_blockers = {
    AWSManagedRulesAmazonIpReputationList = 4
    AWSManagedRulesKnownBadInputsRuleSet  = 2
  }

  waf_rule_sets_priority_observers = {
    AWSManagedRulesSQLiRuleSet      = 6
    AWSManagedRulesAnonymousIpList  = 8
  }

  waf_bot_control_priority = 10

  realtime_log_name_suffix = var.cloudfront_realtime_log_name_suffix != null ? var.cloudfront_realtime_log_name_suffix : var.cloudfront_name
  realtime_log_base_name   = "${local.name_prefix}-${local.realtime_log_name_suffix}-rt-logs"
  realtime_log_stream_name = substr(local.realtime_log_base_name, 0, 128)
}
