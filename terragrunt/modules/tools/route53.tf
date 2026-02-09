moved {
  from = aws_route53_record.clamav_rest_to_entrypoint_alias
  to   = aws_route53_record.clamav_rest
}
resource "aws_route53_record" "clamav_rest" {
  zone_id = var.public_hosted_zone_id
  name    = var.tools_configs.clamav_rest.name
  type    = "CNAME"
  ttl     = 60

  records = [aws_lb.tools.dns_name]
}

moved {
  from = aws_route53_record.cloud_beaver_to_entrypoint_alias
  to   = aws_route53_record.cloud_beaver
}
resource "aws_route53_record" "cloud_beaver" {
  zone_id = var.public_hosted_zone_id
  name    = var.tools_configs.cloud_beaver.name
  type    = "CNAME"
  ttl     = 60

  records = [aws_lb.tools.dns_name]
}

moved {
  from = aws_route53_record.healthcheck_to_entrypoint_alias
  to   = aws_route53_record.healthcheck
}
resource "aws_route53_record" "healthcheck" {
  zone_id = var.public_hosted_zone_id
  name    = var.healthcheck_config.name
  type    = "CNAME"
  ttl     = 60

  records = [aws_lb.tools.dns_name]
}

resource "aws_route53_record" "opensearch_admin" {
  zone_id = var.public_hosted_zone_id
  name    = var.opensearch_admin_config.name
  type    = "CNAME"
  ttl     = 60

  records = [aws_lb.tools.dns_name]
}

resource "aws_route53_record" "opensearch_gateway" {
  zone_id = var.public_hosted_zone_id
  name    = var.opensearch_gateway_config.name
  type    = "CNAME"
  ttl     = 60

  records = [aws_lb.tools.dns_name]
}

resource "aws_route53_record" "s3_uploader" {
  zone_id = var.public_hosted_zone_id
  name    = var.s3_uploader_config.name
  type    = "CNAME"
  ttl     = 60

  records = [aws_lb.tools.dns_name]
}
