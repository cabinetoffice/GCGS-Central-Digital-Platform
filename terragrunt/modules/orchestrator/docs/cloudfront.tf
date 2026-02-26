resource "aws_cloudfront_distribution" "docs" {
  count = var.cloudfront_enabled ? 1 : 0

  enabled             = true
  is_ipv6_enabled     = true
  comment             = "${local.name_prefix} documentation"
  default_root_object = "index.html"
  price_class         = var.cloudfront_price_class
  aliases             = var.cloudfront_custom_domain_enabled ? [var.docs_domain_name] : []

  origin {
    domain_name = local.website_domain
    origin_id   = "docs-s3-website"

    custom_origin_config {
      http_port              = 80
      https_port             = 443
      origin_protocol_policy = "http-only"
      origin_ssl_protocols   = ["TLSv1.2"]
    }
  }

  default_cache_behavior {
    target_origin_id       = "docs-s3-website"
    viewer_protocol_policy = "redirect-to-https"
    allowed_methods        = ["GET", "HEAD"]
    cached_methods         = ["GET", "HEAD"]
    compress               = true

    forwarded_values {
      query_string = false
      cookies {
        forward = "none"
      }
    }
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  viewer_certificate {
    cloudfront_default_certificate = var.cloudfront_custom_domain_enabled == false
    acm_certificate_arn            = var.cloudfront_custom_domain_enabled ? var.cloudfront_acm_certificate_arn : null
    ssl_support_method             = var.cloudfront_custom_domain_enabled ? "sni-only" : null
    minimum_protocol_version       = var.cloudfront_custom_domain_enabled ? "TLSv1.2_2021" : null
  }

  tags = var.tags
}
