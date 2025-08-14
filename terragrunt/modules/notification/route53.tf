resource "aws_route53_record" "dkim" {
  for_each = {
    for pair in flatten([
      for domain in local.effective_mail_from_domains : [
        for i in range(3) : {
          key    = "${domain}-${i}"
          domain = domain
          index  = i
        }
      ]
      ]) : pair.key => {
      domain = pair.domain
      index  = pair.index
    }
  }

  name    = "${aws_ses_domain_dkim.this[each.value.domain].dkim_tokens[each.value.index]}.${each.value.domain}"
  type    = "CNAME"
  ttl     = 300
  zone_id = var.public_hosted_zone_id
  records = ["${aws_ses_domain_dkim.this[each.value.domain].dkim_tokens[each.value.index]}.dkim.amazonses.com"]
}

resource "aws_route53_record" "ses_verification" {
  for_each = aws_ses_domain_identity.this

  name    = "_amazonses.${each.key}"
  type    = "TXT"
  ttl     = 300
  zone_id = var.public_hosted_zone_id
  records = [each.value.verification_token]
}

