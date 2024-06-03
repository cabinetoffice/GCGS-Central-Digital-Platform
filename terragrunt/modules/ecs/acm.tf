resource "aws_acm_certificate" "this" {
  domain_name       = var.public_hosted_zone_fqdn
  subject_alternative_names = ["*.${var.public_hosted_zone_fqdn}"] # @todo (ABN) Restrict to service sub-domains
  validation_method = "DNS"

  tags = merge(var.tags, { Name = var.public_hosted_zone_fqdn })

  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_acm_certificate_validation" "this" {
  certificate_arn         = aws_acm_certificate.this.arn
  validation_record_fqdns = [for record in aws_acm_certificate.this.domain_validation_options : record.resource_record_name]
}

resource "aws_route53_record" "cert_validation" {
  for_each = {
    for dvo in aws_acm_certificate.this.domain_validation_options : dvo.domain_name => {
      name   = dvo.resource_record_name
      record = dvo.resource_record_value
      type   = dvo.resource_record_type
    }
  }
  allow_overwrite = true
  name            = each.value.name
  records         = [each.value.record]
  ttl             = 60
  type            = each.value.type
  zone_id         = var.public_hosted_zone_id
}
