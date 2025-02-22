resource "aws_route53_record" "clamav_rest_to_entrypoint_alias" {
  zone_id = var.public_hosted_zone_id
  name    = var.tools_configs.clamav_rest.name
  type    = "CNAME"
  ttl     = 60

  records = [var.ecs_alb_dns_name]
}

resource "aws_route53_record" "healthcheck_to_entrypoint_alias" {
  zone_id = var.public_hosted_zone_id
  name    = var.healthcheck_config.name
  type    = "CNAME"
  ttl     = 60

  records = [var.ecs_alb_dns_name]
}

resource "aws_route53_record" "pgadmin_to_entrypoint_alias" {
  zone_id = var.public_hosted_zone_id
  name    = var.pgadmin_config.name
  type    = "CNAME"
  ttl     = 60

  records = [var.ecs_alb_dns_name]
}
