resource "aws_route53_zone" "data_platform" {
  count = var.is_production ? 1 : 0

  name = replace(var.product.public_hosted_zone, "supplier-information", "data-platform")
  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}

