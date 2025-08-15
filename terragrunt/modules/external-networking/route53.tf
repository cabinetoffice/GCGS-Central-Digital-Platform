resource "aws_route53_zone" "data_platform" {

  count = var.is_production ? 1 : 0

  name = replace(var.product.public_hosted_zone, "supplier-information", "data-platform")
  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_route53_record" "www_to_fts" {
  count = var.fts_azure_frontdoor == null ? 0 : 1

  zone_id = var.core_hosted_zone_id
  name    = "www"
  type    = "CNAME"
  ttl     = 60

  records = [var.fts_azure_frontdoor]
}

