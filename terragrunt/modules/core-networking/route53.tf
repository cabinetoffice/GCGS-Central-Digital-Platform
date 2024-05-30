resource "aws_route53_zone" "public" {

  name = var.product.public_hosted_zone
  tags = merge(var.tags, { environment : "all", state_location : "all" })

  lifecycle {
    prevent_destroy = false
  }
}
