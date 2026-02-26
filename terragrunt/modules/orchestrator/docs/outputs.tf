output "bucket_arn" {
  value = module.docs_bucket.arn
}

output "bucket_name" {
  value = module.docs_bucket.bucket
}

output "cloudfront_distribution_id" {
  value = var.cloudfront_enabled ? aws_cloudfront_distribution.docs[0].id : null
}

output "cloudfront_domain_name" {
  value = var.cloudfront_enabled ? aws_cloudfront_distribution.docs[0].domain_name : null
}

output "docs_acm_certificate_arn" {
  value = length(aws_acm_certificate.docs) > 0 ? aws_acm_certificate.docs[0].arn : null
}

output "docs_acm_validation_records" {
  value = length(aws_acm_certificate.docs) > 0 ? [
    for dvo in aws_acm_certificate.docs[0].domain_validation_options : {
      name  = dvo.resource_record_name
      type  = dvo.resource_record_type
      value = dvo.resource_record_value
    }
  ] : []
}

output "publisher_role_arn" {
  value = aws_iam_role.docs_publisher.arn
}

output "publisher_role_name" {
  value = aws_iam_role.docs_publisher.name
}
