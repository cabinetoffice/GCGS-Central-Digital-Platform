provider "aws" {
  alias  = "virginia"
  region = "us-east-1"
}

resource "aws_wafv2_ip_set" "cloudfront_allowlist" {
  count = var.cloudfront_enabled && var.waf_enabled && length(local.waf_allowed_ip_list) > 0 ? 1 : 0

  provider           = aws.virginia
  name               = "${local.name_prefix}-${var.environment}-cf-known-ips"
  description        = "IP Set to explicitly allow known trusted IPs for CloudFront"
  scope              = "CLOUDFRONT"
  ip_address_version = "IPV4"
  addresses          = local.waf_allowed_ip_list

  tags = merge(
    { Name = "${local.name_prefix}-${var.environment}-cf-known-ips" },
    var.tags
  )
}

resource "aws_wafv2_web_acl" "cloudfront" {
  count = var.cloudfront_enabled && var.waf_enabled ? 1 : 0

  provider    = aws.virginia
  name        = "${local.name_prefix}-${var.environment}-cf-acl"
  description = "${local.name_prefix} CloudFront Web ACL"
  scope       = "CLOUDFRONT"

  default_action {
    allow {}
  }

  custom_response_body {
    key          = "${local.name_prefix}_${var.environment}_blocked_request"
    content      = "Access denied"
    content_type = "TEXT_PLAIN"
  }

  dynamic "rule" {
    for_each = length(local.waf_allowed_ip_list) > 0 ? [1] : []
    content {
      name     = "${local.name_prefix}-${var.environment}-allow-known-ips"
      priority = 0

      action {
        allow {}
      }

      statement {
        ip_set_reference_statement {
          arn = aws_wafv2_ip_set.cloudfront_allowlist[0].arn
        }
      }

      visibility_config {
        cloudwatch_metrics_enabled = true
        metric_name                = "${local.name_prefix}-${var.environment}-allow-known-ips"
        sampled_requests_enabled   = true
      }
    }
  }

  dynamic "rule" {
    for_each = local.waf_rule_sets_priority_blockers
    content {
      name     = "${local.name_prefix}-${var.environment}-${rule.key}"
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
        metric_name                = "${local.name_prefix}-${var.environment}-${rule.key}"
        sampled_requests_enabled   = true
      }
    }
  }

  dynamic "rule" {
    for_each = local.waf_rule_sets_priority_observers
    content {
      name     = "${local.name_prefix}-${var.environment}-${rule.key}"
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
        metric_name                = "${local.name_prefix}-${var.environment}-${rule.key}"
        sampled_requests_enabled   = true
      }
    }
  }

  dynamic "rule" {
    for_each = var.waf_bot_control_enabled ? [1] : []
    content {
      name     = "${local.name_prefix}-${var.environment}-bot-control"
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
        metric_name                = "${local.name_prefix}-${var.environment}-bot-control"
        sampled_requests_enabled   = true
      }
    }
  }

  visibility_config {
    cloudwatch_metrics_enabled = true
    metric_name                = "${local.name_prefix}-${var.environment}-cf-waf"
    sampled_requests_enabled   = true
  }

  tags = merge(
    { Name = "${local.name_prefix}-${var.environment}-cf-acl" },
    var.tags
  )
}
