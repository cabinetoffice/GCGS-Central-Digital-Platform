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

