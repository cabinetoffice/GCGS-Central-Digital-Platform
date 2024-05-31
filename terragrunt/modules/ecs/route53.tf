resource "aws_route53_record" "ecs_alb" {
  for_each = toset(local.services)

  zone_id = var.public_hosted_zone_id
  name    = each.value
  type    = "CNAME"
  ttl     = 60

  records = [aws_lb.ecs.dns_name]
}
