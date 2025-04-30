import {
  id = "Z04057483Q7XT55RB4P27"
  to = aws_route53_zone.public
}
resource "aws_route53_zone" "public" {

  name = var.product.public_hosted_zone
  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_route53_zone" "production_private_beta" {
  count = var.is_production ? 1 : 0

  name = "private-beta.find-tender.service.gov.uk"
  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_route53_record" "development_delegation"{
  count = var.is_production ? 1 : 0

  name    = "dev.${aws_route53_zone.public.name}"
  type    = "NS"
  ttl     = 300
  zone_id = aws_route53_zone.public.id

  records = [
     "ns-814.awsdns-37.net.",
     "ns-1119.awsdns-11.org.",
     "ns-318.awsdns-39.com.",
     "ns-1696.awsdns-20.co.uk.",
  ]
}

