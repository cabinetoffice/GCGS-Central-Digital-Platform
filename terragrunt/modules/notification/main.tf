resource "aws_ses_domain_identity" "this" {
  for_each = toset(local.effective_mail_from_domains)

  domain   = each.value
}

resource "aws_ses_domain_dkim" "this" {
  for_each = aws_ses_domain_identity.this

  domain   = each.value.domain

  depends_on = [aws_ses_domain_identity.this]
}

resource "aws_ses_identity_policy" "send_policy" {
  for_each = toset(local.effective_mail_from_domains)

  identity = aws_ses_domain_identity.this[each.key].arn
  name     = "${local.name_prefix}-AllowSESSend-${replace(replace(each.key, ".", "-"), "/^.{0,64}/", "")}"
  policy   = data.aws_iam_policy_document.ses_send[each.key].json
}

resource "aws_ses_configuration_set" "json_logging" {
  name = "${local.logging_prefix}-config-set"
}

resource "aws_ses_event_destination" "json_logging" {
  name                   = "${local.logging_prefix}-to-sns"
  configuration_set_name = aws_ses_configuration_set.json_logging.name
  enabled                = true

  matching_types = var.ses_logging_event_types

  sns_destination {
    topic_arn = aws_sns_topic.ses_json_events.arn
  }
}
