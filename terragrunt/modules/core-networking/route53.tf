resource "aws_route53_zone" "public" {

  name = var.product.public_hosted_zone
  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_route53_zone" "cfs" {
  count = var.environment == "orchestrator" ? 0 : 1

  name = local.hosted_zones.cfs[var.environment]
  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_route53_zone" "fts" {
  count = var.environment == "orchestrator" ? 0 : 1

  name = local.hosted_zones.fts[var.environment]
  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}

