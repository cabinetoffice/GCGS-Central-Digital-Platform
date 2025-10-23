resource "aws_route53_zone" "data_platform" {
  count = var.is_production ? 1 : 0

  name = replace(var.product.public_hosted_zone, "supplier-information", "data-platform")
  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_route53_record" "commercial_ai" {
  count = var.is_production ? 1 : 0

  zone_id = var.core_hosted_zone_id
  name    = "commercial-ai"
  type    = "CNAME"
  ttl     = 60

  records = ["d1kry5jfpfm6f7.cloudfront.net"]
}
