provider "aws" {
  alias  = "virginia"
  region = "us-east-1"
}

resource "aws_wafv2_web_acl" "cloudfront" {
  count = var.cloudfront_enabled && var.waf_enabled ? 1 : 0

  provider    = aws.virginia
  name        = "${local.cf_name_prefix}-acl"
  description = "${local.name_prefix} CloudFront Web ACL"
  scope       = "CLOUDFRONT"

  default_action {
    allow {}
  }

  custom_response_body {
    key          = "${local.name_prefix}_${var.cloudfront_name}_blocked_request"
    content      = "Access denied"
    content_type = "TEXT_PLAIN"
  }

  dynamic "rule" {
    for_each = local.waf_rule_sets_priority_blockers
    content {
      name     = "${local.cf_name_prefix}-${rule.key}"
      priority = rule.value

      override_action {
        none {}
      }

      statement {
        managed_rule_group_statement {
          name        = rule.key
          vendor_name = "AWS"
        }
      }

      visibility_config {
        cloudwatch_metrics_enabled = true
        metric_name                = "${local.cf_name_prefix}-${rule.key}"
        sampled_requests_enabled   = true
      }
    }
  }

  dynamic "rule" {
    for_each = local.waf_rule_sets_priority_observers
    content {
      name     = "${local.cf_name_prefix}-${rule.key}"
      priority = rule.value

      override_action {
        count {}
      }

      statement {
        managed_rule_group_statement {
          name        = rule.key
          vendor_name = "AWS"
        }
      }

      visibility_config {
        cloudwatch_metrics_enabled = true
        metric_name                = "${local.cf_name_prefix}-${rule.key}"
        sampled_requests_enabled   = true
      }
    }
  }

  dynamic "rule" {
    for_each = var.waf_bot_control_enabled ? [1] : []
    content {
      name     = "${local.cf_name_prefix}-bot-control"
      priority = local.waf_bot_control_priority

      override_action {
        none {}
      }

      statement {
        managed_rule_group_statement {
          name        = "AWSManagedRulesBotControlRuleSet"
          vendor_name = "AWS"
        }
      }

      visibility_config {
        cloudwatch_metrics_enabled = true
        metric_name                = "${local.cf_name_prefix}-bot-control"
        sampled_requests_enabled   = true
      }
    }
  }

  visibility_config {
    cloudwatch_metrics_enabled = true
    metric_name                = "${local.cf_name_prefix}-waf"
    sampled_requests_enabled   = true
  }

  tags = merge(
    { Name = "${local.cf_name_prefix}-acl" },
    var.tags
  )
}
