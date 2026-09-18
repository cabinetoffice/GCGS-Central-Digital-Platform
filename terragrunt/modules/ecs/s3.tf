module "s3_bucket_staging" {
  source             = "../s3-bucket"
  bucket_name        = "${local.name_prefix}-${var.environment}-upload-temp-${data.aws_caller_identity.current.account_id}"
  kms_key_admin_role = var.role_terraform_arn
  read_roles         = [var.role_ecs_task_exec_arn]
  write_roles        = [var.role_ecs_task_arn]

  tags = var.tags
}

module "s3_bucket_permanent" {
  source             = "../s3-bucket"
  bucket_name        = "${local.name_prefix}-${var.environment}-upload-${data.aws_caller_identity.current.account_id}"
  kms_key_admin_role = var.role_terraform_arn
  read_roles         = [var.role_ecs_task_exec_arn]
  write_roles        = [var.role_ecs_task_arn]

  tags = var.tags
}

module "s3_bucket_cfs" {
  source             = "../s3-bucket"
  bucket_name        = "${local.name_prefix}-${var.environment}-cfs-${data.aws_caller_identity.current.account_id}"
  enable_encryption  = false
  is_public          = true
  kms_key_admin_role = var.role_terraform_arn
  read_roles         = [var.role_ecs_task_exec_arn]
  write_roles        = [var.role_ecs_task_arn]

  tags = var.tags
}

module "s3_bucket_fts" {
  source             = "../s3-bucket"
  bucket_name        = "${local.name_prefix}-${var.environment}-fts-${data.aws_caller_identity.current.account_id}"
  enable_encryption  = false
  is_public          = true
  kms_key_admin_role = var.role_terraform_arn
  read_roles         = [var.role_ecs_task_exec_arn]
  write_roles        = [var.role_ecs_task_arn]

  cors_rules = [
    {
      allowed_headers = ["*"]
      allowed_methods = ["PUT", "GET", "HEAD"]
      allowed_origins = ["https://s3-uploader.${var.public_domain}"]
      expose_headers  = ["ETag"]
      max_age_seconds = 3000
    }
  ]

  tags = var.tags
}

module "s3_bucket_fts_notice_render_cache" {
  source                 = "../s3-bucket"
  bucket_name            = "${local.name_prefix}-${var.environment}-fts-notice-render-cache-${data.aws_caller_identity.current.account_id}"
  cloudfront_read_access = true
  create_kms_key         = false
  enable_encryption      = true
  kms_key_admin_role     = var.role_terraform_arn
  read_roles             = [var.role_ecs_task_exec_arn]
  sse_algorithm          = "AES256"
  write_roles            = [var.role_ecs_task_arn]

  tags = var.tags
}

module "s3_bucket_ocds_exports" {
  source = "../s3-bucket"

  bucket_name        = "${local.name_prefix}-${var.environment}-ocds-exports-${data.aws_caller_identity.current.account_id}"
  enable_encryption  = false
  kms_key_admin_role = var.role_terraform_arn
  read_roles         = [var.role_ecs_task_exec_arn]
  write_roles        = [var.role_ecs_task_arn]

  tags = var.tags
}

module "s3_bucket_e2e_nightly_dev_reports" {
  source = "../s3-bucket"
  count  = local.e2e_nightly_dev_enabled ? 1 : 0

  bucket_name        = "${local.name_prefix}-${var.environment}-e2e-nightly-dev-reports-${data.aws_caller_identity.current.account_id}"
  kms_key_admin_role = var.role_terraform_arn
  read_roles         = compact([var.role_terraform_arn, var.role_filestash_task_arn])
  write_roles        = [var.role_ecs_task_arn]

  tags = var.tags
}
