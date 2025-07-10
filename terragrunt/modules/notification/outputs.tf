output "dkim_tokens" {
  value       = aws_ses_domain_dkim.this.dkim_tokens
  description = "DKIM tokens for the domain"
}

output "domain_identity_arn" {
  value = aws_ses_domain_identity.this.arn
}

output "ses_verification_record" {
  value = {
    name  = "_amazonses.${aws_ses_domain_identity.this.domain}"
    type  = "TXT"
    value = aws_ses_domain_identity.this.verification_token
  }
  description = "Record to verify SES domain"
}
