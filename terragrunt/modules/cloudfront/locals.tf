locals {
  name_prefix = var.product.resource_name

  origin_bucket_name = "${local.name_prefix}-${var.environment}-cloudfront-origin-${data.aws_caller_identity.current.account_id}"
  log_bucket_name    = "${local.name_prefix}-${var.environment}-cloudfront-logs-${data.aws_caller_identity.current.account_id}"

  default_origin_id = "s3-origin"

  ordered_cache_behaviors = []

  waf_log_group_name = "aws-waf-logs-${local.name_prefix}-${var.environment}-cf"

  normalized_aliases = [for alias in var.cloudfront_aliases : trimsuffix(alias, ".")]
  primary_alias      = length(local.normalized_aliases) > 0 ? local.normalized_aliases[0] : null

  acm_certificate_arn = var.cloudfront_acm_certificate_arn != null ? var.cloudfront_acm_certificate_arn : try(aws_acm_certificate.cloudfront[0].arn, null)

  waf_raw_ip_set_json = try(jsondecode(data.aws_secretsmanager_secret_version.waf_allowed_ips[0].secret_string), [])
  waf_allowed_ip_list = length(local.waf_raw_ip_set_json) > 0 ? [
    for item in local.waf_raw_ip_set_json : item.value if can(item.value)
  ] : []

  waf_rule_sets_priority_blockers = {
    AWSManagedRulesAmazonIpReputationList = 4
    AWSManagedRulesKnownBadInputsRuleSet  = 2
  }

  waf_rule_sets_priority_observers = {
    AWSManagedRulesSQLiRuleSet      = 6
    AWSManagedRulesAnonymousIpList  = 8
  }

  waf_bot_control_priority = 10
}
