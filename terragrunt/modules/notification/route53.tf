resource "aws_route53_record" "dkim" {
  count   = 3
  zone_id = var.public_hosted_zone_id
  name    = "${aws_ses_domain_dkim.this.dkim_tokens[count.index]}.${var.product.public_hosted_zone}"
  type    = "CNAME"
  ttl     = 300
  records = ["${aws_ses_domain_dkim.this.dkim_tokens[count.index]}.dkim.amazonses.com"]
}

resource "aws_route53_record" "mail_from_mx" {
  zone_id = var.public_hosted_zone_id
  name    = "${var.mail_from_subdomain}.${var.product.public_hosted_zone}"
  type    = "MX"
  ttl     = 300
  records = ["10 feedback-smtp.${data.aws_region.current.name}.amazonses.com"]
}

resource "aws_route53_record" "mail_from_spf" {
  zone_id = var.public_hosted_zone_id
  name    = "${var.mail_from_subdomain}.${var.product.public_hosted_zone}"
  type    = "TXT"
  ttl     = 300
  records = ["v=spf1 include:amazonses.com ~all"]
}

resource "aws_route53_record" "ses_verification" {
  zone_id = var.public_hosted_zone_id
  name    = "_amazonses.${var.product.public_hosted_zone}"
  type    = "TXT"
  ttl     = 300
  records = [aws_ses_domain_identity.this.verification_token]
}
