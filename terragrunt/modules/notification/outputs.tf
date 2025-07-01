output "dkim_tokens" {
  value = aws_ses_domain_dkim.this.dkim_tokens
}

output "domain_identity_arn" {
  value = aws_ses_domain_identity.this.arn
}

output "mail_from_domain" {
  value = aws_ses_domain_mail_from.this.mail_from_domain
}
