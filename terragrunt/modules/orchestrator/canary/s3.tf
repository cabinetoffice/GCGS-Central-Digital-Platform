module "s3_bucket_canary" {
  source             = "../../s3-bucket"
  bucket_name        = "${local.name_prefix}-${var.environment}-canary-${data.aws_caller_identity.current.account_id}"
  kms_key_admin_role = var.role_terraform_arn
  read_roles         = [var.role_canary_arn]
  write_roles        = [var.role_canary_arn]

  tags = var.tags
}

