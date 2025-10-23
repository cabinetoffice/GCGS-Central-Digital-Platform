resource "aws_route53_zone" "find_tender_service_gov_uk" {
  count = var.is_production ? 1 : 0

  name = "find-tender.service.gov.uk"
  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_route53_zone" "contractsfinder_service_gov_uk" {
  count = var.is_production ? 1 : 0

  name = "contractsfinder.service.gov.uk"
  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}


resource "aws_route53_record" "development_delegation" {
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

resource "aws_route53_record" "staging_delegation" {
  count = var.is_production ? 1 : 0

  name    = "staging.${aws_route53_zone.public.name}"
  type    = "NS"
  ttl     = 300
  zone_id = aws_route53_zone.public.id

  records = [
    "ns-1329.awsdns-38.org.",
    "ns-1871.awsdns-41.co.uk.",
    "ns-482.awsdns-60.com.",
    "ns-613.awsdns-12.net.",
  ]
}

resource "aws_route53_record" "integration_delegation" {
  count = var.is_production ? 1 : 0

  name    = "integration.${aws_route53_zone.public.name}"
  type    = "NS"
  ttl     = 300
  zone_id = aws_route53_zone.public.id

  records = [
    "ns-1067.awsdns-05.org.",
    "ns-559.awsdns-05.net.",
    "ns-57.awsdns-07.com.",
    "ns-1636.awsdns-12.co.uk.",
  ]
}
