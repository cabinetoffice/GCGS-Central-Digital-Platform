resource "aws_route53_record" "healthcheck_to_entrypoint_alias" {
  zone_id = var.public_hosted_zone_id
  name    = var.environment == "production" ? "${var.healthcheck_config.name}.${local.production_subdomain}" : var.healthcheck_config.name
  type    = "CNAME"
  ttl     = 60

  records = [var.public_domain]
}

resource "aws_route53_record" "pgadmin_to_entrypoint_alias" {
  zone_id = var.public_hosted_zone_id
  name    = var.environment == "production" ? "${var.pgadmin_config.name}.${local.production_subdomain}" : var.pgadmin_config.name
  type    = "CNAME"
  ttl     = 60

  records = [var.public_domain]
}
