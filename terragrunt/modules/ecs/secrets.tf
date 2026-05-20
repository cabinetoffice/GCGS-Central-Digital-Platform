resource "random_uuid" "fts_notice_publish_internal_key" {
  keepers = {
    environment = var.environment
  }
}

resource "aws_secretsmanager_secret" "fts_notice_publish_internal_key" {
  name        = "${local.name_prefix}-fts-notice-publish-internal-key"
  description = "Shared notice publish internal key for FTS legacy PHP and .NET apps"
  tags        = var.tags
}

resource "aws_secretsmanager_secret_version" "fts_notice_publish_internal_key" {
  secret_id     = aws_secretsmanager_secret.fts_notice_publish_internal_key.id
  secret_string = random_uuid.fts_notice_publish_internal_key.result
}
