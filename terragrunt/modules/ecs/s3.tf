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
