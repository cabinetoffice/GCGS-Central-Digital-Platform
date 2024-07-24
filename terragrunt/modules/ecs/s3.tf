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
