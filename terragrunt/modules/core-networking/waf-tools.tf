resource "aws_wafv2_web_acl" "tools" {
  count = var.environment != "orchestrator" ? 1 : 0

  name        = "${local.name_prefix}-acl-tools"
  description = "${local.name_prefix} Web ACL for tools"
  scope       = "REGIONAL" # "CLOUDFRONT" N.Virginia

  default_action {
    block {}
  }

  # @TODO (ABN) verify functionality before deciding which accounts can be open
  # dynamic "default_action" {
  #   for_each = var.environment == "production" ? [1] : []
  #   content {
  #     block {}
  #   }
  # }
  #
  # dynamic "default_action" {
  #   for_each = var.environment != "production" ? [1] : []
  #   content {
  #     allow {}
  #   }
  # }

  custom_response_body {
    key          = "${local.name_prefix}_tools_blocked_request"
    content      = "Tools Access Denied"
    content_type = "TEXT_PLAIN"
  }

  rule {
    name     = "${local.name_prefix}-tools-allow-known-ips"
    priority = 0

    action {
      allow {}
    }

    statement {
      ip_set_reference_statement {
        arn = aws_wafv2_ip_set.tools.arn
      }
    }

    visibility_config {
      cloudwatch_metrics_enabled = true
      metric_name                = "${local.name_prefix}-allow-known-ips"
      sampled_requests_enabled   = true
    }
  }

  visibility_config {
    cloudwatch_metrics_enabled = true
    metric_name                = "${local.name_prefix}-waf-acl-tools"
    sampled_requests_enabled   = true
  }

  tags = merge(
    { Name = "${local.name_prefix}-acl" },
    var.tags
  )

}

resource "aws_wafv2_ip_set" "tools" {
  name               = "${local.name_prefix}-tools-known-ips"
  description        = "IP Set to explicitly allow known trusted IPs to tooling services"
  scope              = "REGIONAL"
  ip_address_version = "IPV4"
  addresses          = local.waf_allowed_ip_list_tools

  tags = merge(
    { Name = "${local.name_prefix}-tools-known-ips" },
    var.tags
  )
}

resource "aws_wafv2_web_acl_logging_configuration" "tools" {
  count = var.environment != "orchestrator" ? 1 : 0

  log_destination_configs = [aws_cloudwatch_log_group.waf_tools.arn]
  resource_arn            = aws_wafv2_web_acl.tools[0].arn

}
