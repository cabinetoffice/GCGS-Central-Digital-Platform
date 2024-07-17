module "s3_bucket" {
  source      = "../s3-bucket"
  bucket_name = "${local.name_prefix}-${var.environment}-ci-artifact-${data.aws_caller_identity.current.account_id}"
  write_roles = [var.ci_role_arn]

  tags = var.tags
}
