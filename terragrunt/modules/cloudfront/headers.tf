resource "aws_cloudfront_response_headers_policy" "security" {
  count = var.cloudfront_response_headers_policy_enabled ? 1 : 0

  name    = "${local.name_prefix}-${var.environment}-security-headers"
  comment = "Security headers for ${local.name_prefix} ${var.environment} CloudFront"

  security_headers_config {
    content_type_options {
      override = true
    }

    frame_options {
      frame_option = "DENY"
      override     = true
    }

    referrer_policy {
      referrer_policy = "strict-origin-when-cross-origin"
      override        = true
    }

    strict_transport_security {
      access_control_max_age_sec = 31536000
      include_subdomains         = true
      preload                    = false
      override                   = true
    }
  }
}
