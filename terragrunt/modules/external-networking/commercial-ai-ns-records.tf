resource "aws_route53_record" "commercial_ai_cert" {
  count = local.commercial_ai_records[var.environment] != null ? 1 : 0

  name    = local.commercial_ai_records[var.environment].validator_name
  records = local.commercial_ai_records[var.environment].validator_records
  ttl     = 60
  type    = "CNAME"
  zone_id = var.core_hosted_zone_id
}

resource "aws_route53_record" "commercial_ai_cname" {
  count = length(try(local.commercial_ai_records[var.environment].target_records, [])) > 0 ? 1 : 0

  name    = "ai"
  records = local.commercial_ai_records[var.environment].target_records
  ttl     = 60
  type    = "CNAME"
  zone_id = var.core_hosted_zone_id
}

resource "aws_route53_record" "commercial_ai_a" {
  zone_id = var.core_hosted_zone_id
  name    = "commercial-ai"
  type    = "A"

  alias {
    name                   = local.commercial_ai_cloudfront_domain
    zone_id                = local.cloudfront_global_hosted_zone_id
    evaluate_target_health = false
  }
}

resource "aws_route53_record" "commercial_ai_aaaa" {
  zone_id = var.core_hosted_zone_id
  name    = "commercial-ai"
  type    = "AAAA"

  alias {
    name                   = local.commercial_ai_cloudfront_domain
    zone_id                = local.cloudfront_global_hosted_zone_id
    evaluate_target_health = false
  }
}
