resource "aws_secretsmanager_secret" "fts_tool_user_passwords" {
  count = local.is_staging ? 1 : 0

  name        = "rds-db-${local.fts_cluster_name}-tool-user-passwords"
  description = "FTS DB tool user passwords (cb_ro_fts, cb_da_fts) used for post-restore password reset in staging"
  tags        = var.tags
}
