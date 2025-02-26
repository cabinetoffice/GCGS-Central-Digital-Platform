resource "aws_route53_record" "grafana_to_alb" {
  zone_id = var.public_hosted_zone_id
  name    = var.grafana_config.name
  type    = "CNAME"
  ttl     = 60

  records = [var.ecs_alb_dns_name]
}
