resource "aws_route53_record" "clamav_rest_to_entrypoint_alias" {
  zone_id = var.public_hosted_zone_id
  name    = var.tools_configs.clamav_rest.name
  type    = "CNAME"
  ttl     = 60

  records = [aws_lb.tools.dns_name]
}

resource "aws_route53_record" "cloud_beaver_to_entrypoint_alias" {
  zone_id = var.public_hosted_zone_id
  name    = var.tools_configs.cloud_beaver.name
  type    = "CNAME"
  ttl     = 60

  records = [aws_lb.tools.dns_name]
}

resource "aws_route53_record" "healthcheck_to_entrypoint_alias" {
  zone_id = var.public_hosted_zone_id
  name    = var.healthcheck_config.name
  type    = "CNAME"
  ttl     = 60

  records = [aws_lb.tools.dns_name]
}
