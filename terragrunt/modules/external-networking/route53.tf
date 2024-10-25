resource "aws_route53_record" "www_to_fts" {
  count = var.fts_azure_frontdoor == null ? 0 : 1

  zone_id = var.public_hosted_zone_id
  name    = "www"
  type    = "CNAME"
  ttl     = 60

  records = [var.fts_azure_frontdoor]
}

