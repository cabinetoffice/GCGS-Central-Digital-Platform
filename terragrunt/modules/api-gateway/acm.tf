provider "aws" {
  alias  = "virginia"
  region = "us-east-1"
}

resource "aws_acm_certificate" "ecs_api" {
  domain_name       = "api.${var.public_hosted_zone_fqdn}"
  provider          = aws.virginia
  validation_method = "DNS"

  tags = merge(var.tags, { Name = "api.${var.public_hosted_zone_fqdn}" })

  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_acm_certificate_validation" "ecs_api" {
  certificate_arn         = aws_acm_certificate.ecs_api.arn
  provider                = aws.virginia
  validation_record_fqdns = [
    for record in aws_acm_certificate.ecs_api.domain_validation_options :record.resource_record_name
  ]
}

resource "aws_route53_record" "cert_api_validation" {
  for_each = {
    for dvo in aws_acm_certificate.ecs_api.domain_validation_options : dvo.domain_name => {
      name   = dvo.resource_record_name
      record = dvo.resource_record_value
      type   = dvo.resource_record_type
    }
  }
  allow_overwrite = true
  name            = each.value.name
  provider        = aws.virginia
  records         = [each.value.record]
  ttl             = 60
  type            = each.value.type
  zone_id         = var.public_hosted_zone_id
}
