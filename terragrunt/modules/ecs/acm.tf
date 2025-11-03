resource "aws_acm_certificate" "this" {
  domain_name               = var.public_domain
  subject_alternative_names = ["*.${var.public_domain}"]
  validation_method         = "DNS"

  tags = merge(var.tags, { Name = var.public_domain })

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

resource "aws_acm_certificate" "private_beta" {
  count = var.is_production ? 1 : 0

  domain_name               = var.private_beta_domain
  subject_alternative_names = ["*.${var.private_beta_domain}"]
  validation_method         = "DNS"

  tags = merge(var.tags, { Name = var.private_beta_domain })

  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_acm_certificate_validation" "private_beta" {
  count = var.is_production ? 1 : 0

  certificate_arn         = aws_acm_certificate.private_beta[0].arn
  validation_record_fqdns = [for record in aws_acm_certificate.private_beta[0].domain_validation_options : record.resource_record_name]
}

resource "aws_route53_record" "private_beta" {
  for_each = var.is_production ? {
    for dvo in aws_acm_certificate.private_beta[0].domain_validation_options : dvo.domain_name => {
      name   = dvo.resource_record_name
      record = dvo.resource_record_value
      type   = dvo.resource_record_type
    }
  } : {}
  allow_overwrite = true
  name            = each.value.name
  records         = [each.value.record]
  ttl             = 60
  type            = each.value.type
  zone_id         = var.private_beta_hosted_zone_id
}

resource "aws_lb_listener_certificate" "private_beta" {
  count = var.is_production ? 1 : 0

  certificate_arn = aws_acm_certificate.private_beta[0].arn
  listener_arn    = local.main_ecs_listener_arn
}
