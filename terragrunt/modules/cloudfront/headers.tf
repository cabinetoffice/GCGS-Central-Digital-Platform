resource "aws_cloudfront_response_headers_policy" "security" {
  count = var.cloudfront_response_headers_policy_enabled ? 1 : 0

  name    = "${local.cf_name_prefix}-security-headers"
  comment = "Security headers for ${local.name_prefix} ${var.cloudfront_name} CloudFront"

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
