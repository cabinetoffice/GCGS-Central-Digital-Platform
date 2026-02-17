resource "aws_route53_record" "commercial_ai_cert" {
  count = local.commercial_ai_records[var.environment] != null ? 1 : 0

  name    = local.commercial_ai_records[var.environment].validator_name
  records = local.commercial_ai_records[var.environment].validator_records
  ttl     = 60
  type    = "CNAME"
  zone_id = var.core_hosted_zone_id
}

resource "aws_route53_record" "commercial_ai_cname" {
  count = length(try(local.commercial_ai_records[var.environment].records, [])) > 0 ? 1 : 0

  name    = local.commercial_ai_records[var.environment].name
  records = local.commercial_ai_records[var.environment].records
  ttl     = 60
  type    = "CNAME"
  zone_id = var.core_hosted_zone_id
}
