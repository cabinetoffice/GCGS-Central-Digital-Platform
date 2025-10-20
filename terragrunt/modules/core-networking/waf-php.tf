resource "aws_wafv2_web_acl" "php" {
  count = var.environment != "orchestrator" ? 1 : 0

  name        = "${local.name_prefix_php}-acl"
  description = "${local.name_prefix_php} Web ACL for special services"
  scope       = "REGIONAL" # "CLOUDFRONT" N.Virginia

  default_action {
    allow {}
  }

  custom_response_body {
    key          = "${local.name_prefix_php}_blocked_request"
    content      = "Access denied"
    content_type = "TEXT_PLAIN"
  }

  rule {
    name     = "${local.name_prefix_php}-allow-known-ips"
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
      metric_name                = "${local.name_prefix_php}-allow-known-ips"
      sampled_requests_enabled   = true
    }
  }

  dynamic "rule" {
    for_each = local.waf_rule_sets_priority_blockers
    content {
      name     = "${local.name_prefix_php}-${rule.key}"
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
        metric_name                = "${local.name_prefix_php}-${rule.key}"
        sampled_requests_enabled   = true
      }
    }
  }

  dynamic "rule" {
    for_each = local.waf_rule_sets_priority_observers
    content {
      name     = "${local.name_prefix_php}-${rule.key}"
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
        metric_name                = "${local.name_prefix_php}-${rule.key}"
        sampled_requests_enabled   = true
      }
    }
  }

  visibility_config {
    cloudwatch_metrics_enabled = true
    metric_name                = "${local.name_prefix_php}-waf-acl"
    sampled_requests_enabled   = true
  }

  tags = merge(
    { Name = "${local.name_prefix_php}-acl" },
    var.tags
  )

}

resource "aws_wafv2_web_acl_logging_configuration" "php" {
  count = var.environment != "orchestrator" ? 1 : 0

  log_destination_configs = [aws_cloudwatch_log_group.waf_php.arn]
  resource_arn            = aws_wafv2_web_acl.php[0].arn

}
