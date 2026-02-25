resource "aws_wafv2_web_acl" "fts" {
  count = var.environment != "orchestrator" ? 1 : 0

  name        = "${local.name_prefix_fts}-acl"
  description = "${local.name_prefix_fts} Web ACL"
  scope       = "REGIONAL"

  default_action {
    allow {}
  }

  custom_response_body {
    key          = "${local.name_prefix_fts}_blocked_request"
    content      = "Access denied"
    content_type = "TEXT_PLAIN"
  }

  custom_response_body {
    key          = "${local.name_prefix_fts}_rate_limit_exceeded"
    content      = "Rate limit of ${local.rate_limit_ocdss_releasepac_kages_count} exceeded. Please retry after ${local.rate_limit_ocdss_releasepac_kages_window_seconds} seconds."
    content_type = "TEXT_PLAIN"
  }

  rule {
    name     = "${local.name_prefix_fts}-allow-known-ips"
    priority = 1

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
      metric_name                = "${local.name_prefix_fts}-allow-known-ips"
      sampled_requests_enabled   = true
    }
  }

  dynamic "rule" {
    for_each = local.waf_rule_sets_priority_blockers
    content {
      name     = "${local.name_prefix_fts}-${rule.key}"
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
        metric_name                = "${local.name_prefix_fts}-${rule.key}"
        sampled_requests_enabled   = true
      }
    }
  }

  dynamic "rule" {
    for_each = local.waf_rule_sets_priority_observers
    content {
      name     = "${local.name_prefix_fts}-${rule.key}"
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
        metric_name                = "${local.name_prefix_fts}-${rule.key}"
        sampled_requests_enabled   = true
      }
    }
  }

  rule {
    name     = "${local.name_prefix_fts}-RateLimitOcdsSReleasePackages"
    priority = 0

    action {
      block {
        custom_response {
          response_code            = 429
          custom_response_body_key = "${local.name_prefix_fts}_rate_limit_exceeded"

          response_header {
            name  = "Retry-After"
            value = local.rate_limit_ocdss_releasepac_kages_window_seconds
          }
        }
      }
    }

    statement {
      rate_based_statement {
        limit                 = local.rate_limit_ocdss_releasepac_kages_count
        evaluation_window_sec = local.rate_limit_ocdss_releasepac_kages_window_seconds
        aggregate_key_type    = "IP"

        scope_down_statement {
          regex_match_statement {
            regex_string = "^/api/[^/]+/ocdsReleasePackages.*$"

            field_to_match {
              uri_path {}
            }

            text_transformation {
              priority = 0
              type     = "NONE"
            }
          }
        }
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = true
      metric_name                = "${local.name_prefix_fts}-RateLimitOcdsSReleasePackages"
      sampled_requests_enabled   = true
    }
  }

  visibility_config {
    cloudwatch_metrics_enabled = true
    metric_name                = "${local.name_prefix_fts}-waf-acl"
    sampled_requests_enabled   = true
  }

  tags = merge(
    { Name = "${local.name_prefix_fts}-acl" },
    var.tags
  )

}

resource "aws_wafv2_web_acl_logging_configuration" "fts" {
  count = var.environment != "orchestrator" ? 1 : 0

  log_destination_configs = [aws_cloudwatch_log_group.waf_fts.arn]
  resource_arn            = aws_wafv2_web_acl.fts[0].arn

}
