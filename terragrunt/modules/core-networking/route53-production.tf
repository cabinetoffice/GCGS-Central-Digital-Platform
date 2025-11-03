resource "aws_route53_record" "cfs_delegations" {
  for_each = var.is_production ? local.delegation_ns_records.cfs : {}

  name    = each.key
  type    = "NS"
  ttl     = 300
  zone_id = aws_route53_zone.cfs.id

  records = each.value
}

resource "aws_route53_record" "fts_delegations" {
  for_each = var.is_production ? local.delegation_ns_records.fts : {}

  name    = each.key
  type    = "NS"
  ttl     = 300
  zone_id = aws_route53_zone.fts.id

  records = each.value
}

resource "aws_route53_record" "sirsi_delegations" {
  for_each = var.is_production ? local.delegation_ns_records.sirsi : {}

  name    = each.key
  type    = "NS"
  ttl     = 300
  zone_id = aws_route53_zone.public.id

  records = each.value
}
