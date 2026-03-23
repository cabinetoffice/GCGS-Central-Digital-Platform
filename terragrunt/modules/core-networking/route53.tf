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

resource "aws_route53_zone" "internal" {
  name = length("internal.${aws_route53_zone.public.name}") > 64 ? "in.${aws_route53_zone.public.name}" : "internal.${aws_route53_zone.public.name}"

  vpc {
    vpc_id = aws_vpc.this.id
  }

  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}
