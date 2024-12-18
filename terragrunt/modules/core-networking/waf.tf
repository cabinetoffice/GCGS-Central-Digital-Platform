resource "aws_wafv2_web_acl" "this" {
  count = var.environment != "orchestrator" ? 1 : 0

  name        = "${local.name_prefix}-acl"
  description = "${local.name_prefix} Web ACL"
  scope       = "REGIONAL" # "CLOUDFRONT" N.Virginia

  default_action {
    allow {}
  }

  custom_response_body {
    key          = "${local.name_prefix}_blocked_request"
    content      = "Access denied"
    content_type = "TEXT_PLAIN"
  }

  rule {
    name     = "${local.name_prefix}-allow-known-ips"
    priority = 0

    action {
      allow {}
    }

    statement {
      ip_set_reference_statement {
        arn = aws_wafv2_ip_set.this.arn
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = true
      metric_name                = "${local.name_prefix}-allow-known-ips"
      sampled_requests_enabled   = true
    }
  }

  dynamic "rule" {
    for_each = local.waf_rule_sets_priority
    content {
      name     = "${local.name_prefix}-${rule.key}"
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
        metric_name                = "${local.name_prefix}-${rule.key}"
        sampled_requests_enabled   = true
      }
    }
  }

  visibility_config {
    cloudwatch_metrics_enabled = true
    metric_name                = "${local.name_prefix}-waf-acl"
    sampled_requests_enabled   = true
  }

  tags = merge(
    { Name = "${local.name_prefix}-acl" },
    var.tags
  )

}

resource "aws_wafv2_ip_set" "this" {
  name               = "${local.name_prefix}-known-ips"
  description        = "IP Set to explicitly allow known trusted IPs, even if flagged as anonymous by AWS Managed Rules."
  scope              = "REGIONAL"
  ip_address_version = "IPV4"
  addresses          = local.waf_allowed_ip_list

  tags = merge(
    { Name = "${local.name_prefix}-known-ips" },
    var.tags
  )
}

resource "aws_wafv2_web_acl_logging_configuration" "this" {
  count = var.environment != "orchestrator" ? 1 : 0

  log_destination_configs = [aws_cloudwatch_log_group.waf.arn]
  resource_arn            = aws_wafv2_web_acl.this[0].arn

  logging_filter {
    default_behavior = "KEEP"
    filter {
      behavior = "DROP"
      requirement = "MEETS_ALL"
      condition {
        action_condition {
          action = "ALLOW"
        }
      }
    }
  }
}
