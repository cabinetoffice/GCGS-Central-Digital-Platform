output "configuration_set_name" {
  value = aws_ses_configuration_set.json_logging.name
}

output "dkim_tokens" {
  value = {
    for domain, res in aws_ses_domain_dkim.this :
    domain => res.dkim_tokens
  }
}

output "domain_identity_arn" {
  value = {
    for domain, res in aws_ses_domain_identity.this :
    domain => res.arn
  }
}

output "ses_verification_records" {
  value = {
    for domain, res in aws_ses_domain_identity.this :
    domain => {
      name  = "_amazonses.${domain}"
      type  = "TXT"
      value = res.verification_token
    }
  }
}

