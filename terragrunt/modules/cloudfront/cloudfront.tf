resource "aws_cloudfront_origin_access_control" "s3" {
  name                              = "${local.cf_name_prefix}-oac"
  description                       = "OAC for ${local.name_prefix} ${var.cloudfront_name}"
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

resource "aws_cloudfront_distribution" "this" {
  count = var.cloudfront_enabled ? 1 : 0

  enabled              = true
  is_ipv6_enabled      = true
  comment              = "${local.name_prefix} ${var.cloudfront_name} CloudFront"
  default_root_object  = var.cloudfront_default_root_object
  price_class          = var.cloudfront_price_class
  aliases              = var.cloudfront_custom_domain_enabled ? var.cloudfront_aliases : []

  origin {
    domain_name              = local.origin_bucket_domain_name
    origin_id                = local.default_origin_id
    origin_access_control_id = aws_cloudfront_origin_access_control.s3.id
    connection_attempts      = 3
    connection_timeout       = 10
    response_completion_timeout = 0

    s3_origin_config {
      origin_access_identity = ""
    }
  }

  default_cache_behavior {
    target_origin_id       = local.default_origin_id
    viewer_protocol_policy = "redirect-to-https"
    allowed_methods        = ["GET", "HEAD"]
    cached_methods         = ["GET", "HEAD"]
    compress               = true
    response_headers_policy_id = var.cloudfront_response_headers_policy_enabled ? aws_cloudfront_response_headers_policy.security[0].id : null
    realtime_log_config_arn     = var.cloudfront_realtime_logs_enabled ? aws_cloudfront_realtime_log_config.this[0].arn : null

    forwarded_values {
      query_string = false
      cookies {
        forward = "none"
      }
    }
  }

  dynamic "ordered_cache_behavior" {
    for_each = local.ordered_cache_behaviors
    content {
      path_pattern           = ordered_cache_behavior.value.path_pattern
      target_origin_id       = ordered_cache_behavior.value.target_origin_id
      viewer_protocol_policy = ordered_cache_behavior.value.viewer_protocol_policy
      allowed_methods        = ordered_cache_behavior.value.allowed_methods
      cached_methods         = ordered_cache_behavior.value.cached_methods
      compress               = ordered_cache_behavior.value.compress
      response_headers_policy_id = var.cloudfront_response_headers_policy_enabled ? aws_cloudfront_response_headers_policy.security[0].id : null
      realtime_log_config_arn     = var.cloudfront_realtime_logs_enabled ? aws_cloudfront_realtime_log_config.this[0].arn : null

      forwarded_values {
        query_string = ordered_cache_behavior.value.forward_query_string
        headers      = ordered_cache_behavior.value.forward_headers
        cookies {
          forward = ordered_cache_behavior.value.forward_cookies
        }
      }
    }
  }

  dynamic "logging_config" {
    for_each = var.cloudfront_logging_enabled ? [1] : []
    content {
      bucket          = aws_s3_bucket.logs.bucket_domain_name
      include_cookies = false
      prefix          = "${local.name_prefix}/"
    }
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  viewer_certificate {
    cloudfront_default_certificate = var.cloudfront_custom_domain_enabled == false
    acm_certificate_arn            = var.cloudfront_custom_domain_enabled ? local.acm_certificate_arn : null
    ssl_support_method             = var.cloudfront_custom_domain_enabled ? "sni-only" : null
    minimum_protocol_version       = var.cloudfront_custom_domain_enabled ? "TLSv1.2_2021" : null
  }

  web_acl_id = var.waf_enabled ? try(aws_wafv2_web_acl.cloudfront[0].arn, null) : null

  tags = var.tags

  depends_on = [
    aws_acm_certificate_validation.cloudfront
  ]
}
