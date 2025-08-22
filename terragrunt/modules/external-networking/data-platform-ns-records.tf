resource "aws_route53_record" "data_platform_open_data" {
  count = var.is_production ? 1 : 0

  name    = "staging.open-data"
  records = ["staging.odi.data-platform.codatplat.com"]
  ttl     = 60
  type    = "CNAME"
  zone_id = aws_route53_zone.data_platform[0].zone_id
}

resource "aws_route53_record" "data_platform_admin" {
  count = var.is_production ? 1 : 0

  name    = "staging.admin"
  records = ["staging.admin.data-platform.codatplat.com"]
  ttl     = 60
  type    = "CNAME"
  zone_id = aws_route53_zone.data_platform[0].zone_id
}
