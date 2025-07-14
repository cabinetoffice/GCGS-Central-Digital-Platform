resource "aws_ses_domain_identity" "this" {
  domain = local.effective_mail_from_domain
}

resource "aws_ses_domain_dkim" "this" {
  domain = aws_ses_domain_identity.this.domain
}

resource "aws_ses_identity_policy" "send_policy" {
  name     = "${local.name_prefix}-AllowSESSend"
  identity = aws_ses_domain_identity.this.arn
  policy   = data.aws_iam_policy_document.ses_send.json
}
