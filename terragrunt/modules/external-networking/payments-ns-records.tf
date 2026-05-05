resource "aws_route53_record" "payments_cert" {
  for_each = local.payments_records[var.environment]

  name    = each.value.validator_name
  records = each.value.validator_records
  ttl     = 60
  type    = "CNAME"
  zone_id = var.core_hosted_zone_id
}

resource "aws_route53_record" "payments_alias" {
  for_each = local.payments_records[var.environment]

  name    = each.value.name
  type    = "A"
  zone_id = var.core_hosted_zone_id

  alias {
    evaluate_target_health = false
    name                   = each.value.records[0]
    zone_id                = each.value.lb_zone_id
  }
}
