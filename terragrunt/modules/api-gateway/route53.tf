resource "aws_api_gateway_domain_name" "ecs_api" {
  domain_name = "api.${var.public_hosted_zone_fqdn}"
  certificate_arn = aws_acm_certificate.ecs_api.arn

  depends_on = [
    aws_acm_certificate.ecs_api
  ]
}


resource "aws_route53_record" "api" {
  zone_id = var.public_hosted_zone_id
  name    = aws_api_gateway_domain_name.ecs_api.domain_name
  type    = "A"

  alias {
    name                   = aws_api_gateway_domain_name.ecs_api.cloudfront_domain_name
    zone_id                = aws_api_gateway_domain_name.ecs_api.cloudfront_zone_id
    evaluate_target_health = true
  }
}
