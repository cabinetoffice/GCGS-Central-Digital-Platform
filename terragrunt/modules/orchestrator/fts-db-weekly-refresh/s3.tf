module "artifacts_bucket" {
  source = "../../s3-bucket"

  bucket_name        = "${local.bucket_prefix}-fts-db-weekly-refresh-artifacts-${data.aws_caller_identity.current.account_id}"
  kms_key_admin_role = var.role_terraform_arn
  write_roles        = [var.ci_pipeline_role_arn, var.ci_build_role_arn]

  tags = var.tags
}
