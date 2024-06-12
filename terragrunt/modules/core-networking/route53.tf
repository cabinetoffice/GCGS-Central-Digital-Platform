resource "aws_route53_zone" "public" {

  name = var.product.public_hosted_zone
  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}
