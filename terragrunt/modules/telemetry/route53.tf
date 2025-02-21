resource "aws_route53_record" "service_to_entrypoint_alias" {
  zone_id = var.public_hosted_zone_id
  name    = var.grafana_config.name
  type    = "CNAME"
  ttl     = 60

  records = var.public_domain
  # records = [replace(var.public_domain, "supplier-information.supplier-information", "supplier-information")]
}
