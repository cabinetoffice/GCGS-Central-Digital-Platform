module "artifacts_bucket" {
  source = "../../s3-bucket"

  # Keep bucket name <= 63 chars (S3 hard limit). Account id suffix preserves uniqueness.
  bucket_name        = "${local.bucket_prefix}-fts-db-weekly-refresh-artifacts-${data.aws_caller_identity.current.account_id}"
  kms_key_admin_role = var.role_terraform_arn
  write_roles        = [var.ci_pipeline_role_arn, var.ci_build_role_arn]

  tags = var.tags
}
