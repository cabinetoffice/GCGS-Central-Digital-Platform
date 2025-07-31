resource "aws_ses_domain_identity" "this" {
  for_each = toset(local.effective_mail_from_domains)

  domain   = each.value
}

resource "aws_ses_domain_dkim" "this" {
  for_each = aws_ses_domain_identity.this

  domain   = each.value.domain
}

resource "aws_ses_identity_policy" "send_policy" {
  for_each = toset(local.effective_mail_from_domains)

  identity = aws_ses_domain_identity.this[each.key].arn
  name     = "${local.name_prefix}-AllowSESSend-${replace(replace(each.key, ".", "-"), "/^.{0,64}/", "")}"
  policy   = data.aws_iam_policy_document.ses_send[each.key].json
}

