resource "aws_route53_record" "ecs_alb" {
  for_each = toset(local.services)

  zone_id = var.public_hosted_zone_id
  name    = each.value
  type    = "CNAME"
  ttl     = 60

  records = [aws_lb.ecs.dns_name]
}

resource "aws_route53_record" "ecs_alb_frontend_alias" {

  name    = var.product.public_hosted_zone
  type    = "A"
  zone_id = var.public_hosted_zone_id

  alias {
    evaluate_target_health = true
    name                   = aws_lb.ecs.dns_name
    zone_id                = aws_lb.ecs.zone_id
  }
}
