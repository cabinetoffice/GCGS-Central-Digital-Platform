resource "aws_route53_record" "dkim" {
  count   = local.manage_dns_records ? 3 : 0

  zone_id = var.public_hosted_zone_id
  name    = "${aws_ses_domain_dkim.this.dkim_tokens[count.index]}.${var.product.public_hosted_zone}"
  type    = "CNAME"
  ttl     = 300
  records = ["${aws_ses_domain_dkim.this.dkim_tokens[count.index]}.dkim.amazonses.com"]
}

resource "aws_route53_record" "ses_verification" {
  count   = local.manage_dns_records ? 1 : 0

  zone_id = var.public_hosted_zone_id
  name    = "_amazonses.${var.product.public_hosted_zone}"
  type    = "TXT"
  ttl     = 300
  records = [aws_ses_domain_identity.this.verification_token]
}
