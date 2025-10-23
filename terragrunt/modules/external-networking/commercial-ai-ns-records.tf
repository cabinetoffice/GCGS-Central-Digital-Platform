resource "aws_route53_record" "commercial_ai_cert" {
  count = var.is_production ? 1 : 0

  name    = "_7fe1a72107ce559d9bf4cd43164da019.commercial-ai.supplier-information.find-tender.service.gov.uk."
  records = ["_1616d9f5d97e70042aafc1b4ecb98fab.xlfgrmvvlj.acm-validations.aws."]
  ttl     = 60
  type    = "CNAME"
  zone_id = var.core_hosted_zone_id
}
