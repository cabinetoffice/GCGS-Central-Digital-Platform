resource "aws_ses_domain_identity" "this" {
  domain = var.product.public_hosted_zone
}

resource "aws_ses_domain_dkim" "this" {
  domain = aws_ses_domain_identity.this.domain
}

resource "aws_ses_domain_mail_from" "this" {
  domain            = var.product.public_hosted_zone
  mail_from_domain  = "${var.mail_from_subdomain}.${var.product.public_hosted_zone}"
  behavior_on_mx_failure = "UseDefaultValue"
}

resource "aws_ses_identity_policy" "send_policy" {
  name     = "${local.name_prefix}-AllowSESSend"
  identity = aws_ses_domain_identity.this.arn
  policy   = data.aws_iam_policy_document.ses_send.json
}
