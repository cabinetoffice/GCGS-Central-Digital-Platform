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

  custom_response_body {
    key          = "${local.name_prefix_php}_rate_limit_exceeded"
    content      = "Rate limit of ${local.rate_limit_ocdss_releasepac_kages_count} exceeded. Please retry after ${local.rate_limit_ocdss_releasepac_kages_window_seconds} seconds."
    content_type = "TEXT_PLAIN"
  }

  rule {
    name     = "${local.name_prefix_php}-allow-known-ips"
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
      metric_name                = "${local.name_prefix_php}-allow-known-ips"
      sampled_requests_enabled   = true
    }
  }

  rule {
    name     = "${local.name_prefix_php}-block-bots-ua"
    priority = 3

    action {
      block {}
    }

    statement {
      and_statement {
        statement {
          regex_match_statement {
            regex_string = local.waf_php_bot_block_paths

            field_to_match {
              uri_path {}
            }

            text_transformation {
              priority = 0
              type     = "LOWERCASE"
            }
          }
        }

        statement {
          regex_match_statement {
            regex_string = ".*(chatgpt-user|gptbot|oai-searchbot|amazonbot|meta-externalagent|procurementextractor|deno|supabaseedgeruntime|supabase).*"

            field_to_match {
              single_header {
                name = "user-agent"
              }
            }

            text_transformation {
              priority = 0
              type     = "LOWERCASE"
            }
          }
        }
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = true
      metric_name                = "${local.name_prefix_php}-block-bots-ua"
      sampled_requests_enabled   = true
    }
  }

  rule {
    name     = "${local.name_prefix_php}-bot-control-notice"
    priority = 5

    override_action {
      none {}
    }

    statement {
      managed_rule_group_statement {
        name        = "AWSManagedRulesBotControlRuleSet"
        vendor_name = "AWS"

        rule_action_override {
          name = "CategoryAI"
          action_to_use {
            block {}
          }
        }
        rule_action_override {
          name = "CategoryAdvertising"
          action_to_use {
            block {}
          }
        }
        rule_action_override {
          name = "CategoryArchiver"
          action_to_use {
            block {}
          }
        }
        rule_action_override {
          name = "CategoryContentFetcher"
          action_to_use {
            block {}
          }
        }
        rule_action_override {
          name = "CategoryLinkChecker"
          action_to_use {
            block {}
          }
        }
        rule_action_override {
          name = "CategoryMiscellaneous"
          action_to_use {
            block {}
          }
        }
        rule_action_override {
          name = "CategoryMonitoring"
          action_to_use {
            block {}
          }
        }
        rule_action_override {
          name = "CategoryPagePreview"
          action_to_use {
            block {}
          }
        }
        rule_action_override {
          name = "CategoryScrapingFramework"
          action_to_use {
            block {}
          }
        }
        rule_action_override {
          name = "CategorySearchEngine"
          action_to_use {
            block {}
          }
        }
        rule_action_override {
          name = "CategorySecurity"
          action_to_use {
            block {}
          }
        }
        rule_action_override {
          name = "CategorySeo"
          action_to_use {
            block {}
          }
        }
        rule_action_override {
          name = "CategorySocialMedia"
          action_to_use {
            block {}
          }
        }
        rule_action_override {
          name = "CategoryWebhooks"
          action_to_use {
            block {}
          }
        }

        scope_down_statement {
          regex_match_statement {
            regex_string = local.waf_php_notice_paths_all

            field_to_match {
              uri_path {}
            }

            text_transformation {
              priority = 0
              type     = "LOWERCASE"
            }
          }
        }
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = true
      metric_name                = "${local.name_prefix_php}-bot-control-notice"
      sampled_requests_enabled   = true
    }
  }

  rule {
    name     = "${local.name_prefix_php}-block-verified-bots-notice"
    priority = 7

    action {
      block {}
    }

    statement {
      and_statement {
        statement {
          regex_match_statement {
            regex_string = local.waf_php_notice_paths_all

            field_to_match {
              uri_path {}
            }

            text_transformation {
              priority = 0
              type     = "LOWERCASE"
            }
          }
        }

        statement {
          label_match_statement {
            scope = "LABEL"
            key   = "awswaf:managed:aws:bot-control:bot:verified"
          }
        }
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = true
      metric_name                = "${local.name_prefix_php}-block-verified-bots-notice"
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

  rule {
    name     = "${local.name_prefix_php}-RateLimitOcdsSReleasePackages"
    priority = 0

    action {
      block {
        custom_response {
          response_code            = 429
          custom_response_body_key = "${local.name_prefix_php}_rate_limit_exceeded"

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
      metric_name                = "${local.name_prefix_php}-RateLimitOcdsSReleasePackages"
      sampled_requests_enabled   = true
    }
  }

  rule {
    name     = "${local.name_prefix_php}-RateLimitSearchResultsAndNotice"
    priority = 9

    action {
      block {
        custom_response {
          response_code            = 429
          custom_response_body_key = "${local.name_prefix_php}_rate_limit_exceeded"

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
            regex_string = local.waf_php_rate_limit_paths

            field_to_match {
              uri_path {}
            }

            text_transformation {
              priority = 0
              type     = "LOWERCASE"
            }
          }
        }
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = true
      metric_name                = "${local.name_prefix_php}-RateLimitSearchResultsAndNotice"
      sampled_requests_enabled   = true
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
