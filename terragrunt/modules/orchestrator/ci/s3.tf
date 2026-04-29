module "s3_bucket" {
  source             = "../../s3-bucket"
  bucket_name        = "${local.name_prefix}-${var.environment}-ci-artifact-${data.aws_caller_identity.current.account_id}"
  kms_key_admin_role = var.role_terraform_arn
  write_roles        = [var.ci_role_arn]

  tags = var.tags
}

module "fts_db_backup_bucket" {
  source             = "../../s3-bucket"
  bucket_name        = "${local.name_prefix}-fts-test-db-backup-${data.aws_caller_identity.current.account_id}"
  kms_key_admin_role = var.role_terraform_arn
  read_roles         = [aws_iam_role.fts_github_oidc_read_db_backup.arn]

  tags = var.tags
}
