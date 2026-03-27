output "cloudfront_distribution_id" {
  value = var.cloudfront_enabled ? aws_cloudfront_distribution.this[0].id : null
}

output "cloudfront_domain_name" {
  value = var.cloudfront_enabled ? aws_cloudfront_distribution.this[0].domain_name : null
}

output "cloudfront_hosted_zone_id" {
  value = var.cloudfront_enabled ? aws_cloudfront_distribution.this[0].hosted_zone_id : null
}

output "cloudfront_waf_acl_arn" {
  value = var.cloudfront_enabled && var.waf_enabled ? aws_wafv2_web_acl.cloudfront[0].arn : null
}

output "cloudfront_origin_bucket_name" {
  value = aws_s3_bucket.origin.bucket
}

output "cloudfront_log_bucket_name" {
  value = aws_s3_bucket.logs.bucket
}
