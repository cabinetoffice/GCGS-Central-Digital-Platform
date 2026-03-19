resource "aws_route53_record" "payments_cert" {
  count = local.payments_records[var.environment] != null ? 1 : 0

  name    = local.payments_records[var.environment].validator_name
  records = local.payments_records[var.environment].validator_records
  ttl     = 60
  type    = "CNAME"
  zone_id = var.core_hosted_zone_id
}

resource "aws_route53_record" "payments_alias" {
  count = length(try(local.payments_records[var.environment].records, [])) > 0 ? 1 : 0

  name    = local.payments_records[var.environment].name
  type    = "A"
  zone_id = var.core_hosted_zone_id

  alias {
    evaluate_target_health = false
    name                   = local.payments_records[var.environment].records[0]
    zone_id                = local.payments_records[var.environment].lb_zone_id
  }
}
