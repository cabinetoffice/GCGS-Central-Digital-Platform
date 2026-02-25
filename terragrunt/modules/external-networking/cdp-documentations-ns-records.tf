resource "aws_route53_record" "docs_cert" {
  count = local.docs_records[var.environment] != null && local.docs_records[var.environment].validator_name != null ? 1 : 0

  name    = local.docs_records[var.environment].validator_name
  records = local.docs_records[var.environment].validator_records
  ttl     = 60
  type    = "CNAME"
  zone_id = var.core_hosted_zone_id
}

resource "aws_route53_record" "docs_cname" {
  count = length(try(local.docs_records[var.environment].records, [])) > 0 ? 1 : 0

  name    = local.docs_records[var.environment].name
  records = local.docs_records[var.environment].records
  ttl     = 60
  type    = "CNAME"
  zone_id = var.core_hosted_zone_id
}
