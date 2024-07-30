resource "aws_route53_record" "healthcheck" {
  zone_id = var.public_hosted_zone_id
  name    = var.healthcheck_config.name
  type    = "CNAME"
  ttl     = 60

  records = [var.ecs_lb_dns_name]
}

resource "aws_route53_record" "pgadmin" {
  zone_id = var.public_hosted_zone_id
  name    = var.pgadmin_config.name
  type    = "CNAME"
  ttl     = 60

  records = [var.ecs_lb_dns_name]
}
