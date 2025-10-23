resource "aws_route53_zone" "public" {

  name = var.product.public_hosted_zone
  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_route53_zone" "production_private_beta" {
  count = var.is_production ? 1 : 0

  name = "private-beta.find-tender.service.gov.uk"
  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}
