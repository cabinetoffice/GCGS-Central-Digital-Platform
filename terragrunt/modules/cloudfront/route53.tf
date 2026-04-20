resource "aws_acm_certificate" "cloudfront" {
  count = var.cloudfront_custom_domain_enabled && var.cloudfront_acm_certificate_arn == null ? 1 : 0

  provider          = aws.virginia
  domain_name       = local.primary_alias
  subject_alternative_names = length(local.normalized_aliases) > 1 ? slice(local.normalized_aliases, 1, length(local.normalized_aliases)) : []
  validation_method = "DNS"

  tags = merge(var.tags, { Name = local.primary_alias })

  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_route53_record" "cloudfront_validation" {
  for_each = var.cloudfront_custom_domain_enabled && var.cloudfront_acm_certificate_arn == null ? {
    for dvo in aws_acm_certificate.cloudfront[0].domain_validation_options : dvo.domain_name => {
      name   = dvo.resource_record_name
      record = dvo.resource_record_value
      type   = dvo.resource_record_type
    }
  } : {}

  zone_id = var.cloudfront_route53_zone_id
  name    = each.value.name
  type    = each.value.type
  ttl     = 60
  records = [each.value.record]
}

resource "aws_acm_certificate_validation" "cloudfront" {
  count = var.cloudfront_custom_domain_enabled && var.cloudfront_acm_certificate_arn == null ? 1 : 0

  provider                = aws.virginia
  certificate_arn         = aws_acm_certificate.cloudfront[0].arn
  validation_record_fqdns = [for record in aws_route53_record.cloudfront_validation : record.fqdn]
}

resource "aws_route53_record" "cloudfront_alias_a" {
  for_each = var.cloudfront_custom_domain_enabled && var.cloudfront_manage_route53 ? toset(local.normalized_aliases) : []

  zone_id = var.cloudfront_route53_zone_id
  name    = each.value
  type    = "A"

  alias {
    name                   = aws_cloudfront_distribution.this[0].domain_name
    zone_id                = aws_cloudfront_distribution.this[0].hosted_zone_id
    evaluate_target_health = false
  }
}

resource "aws_route53_record" "cloudfront_alias_aaaa" {
  for_each = var.cloudfront_custom_domain_enabled && var.cloudfront_manage_route53 ? toset(local.normalized_aliases) : []

  zone_id = var.cloudfront_route53_zone_id
  name    = each.value
  type    = "AAAA"

  alias {
    name                   = aws_cloudfront_distribution.this[0].domain_name
    zone_id                = aws_cloudfront_distribution.this[0].hosted_zone_id
    evaluate_target_health = false
  }
}
