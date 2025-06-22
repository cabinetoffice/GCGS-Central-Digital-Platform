import {
  id = "${local.name_prefix}-${var.environment}-deprecated-db-backup-${data.aws_caller_identity.current.account_id}"
  to = module.deprecated_db_backup.aws_s3_bucket.this
}

module "deprecated_db_backup" {
  source             = "../s3-bucket"
  bucket_name        = "${local.name_prefix}-${var.environment}-deprecated-db-backup-${data.aws_caller_identity.current.account_id}"
  kms_key_admin_role = var.role_terraform_arn
  read_roles         = [var.role_rds_backup_arn]
  write_roles        = [var.role_rds_backup_arn]

  tags = var.tags
}

import {
  id = "${local.name_prefix}-rare-handover-bucket-${data.aws_caller_identity.current.account_id}"
  to = module.sql_dump_upload_bucket.aws_s3_bucket.this
}

module "sql_dump_upload_bucket" {
  source = "../s3-bucket"

  bucket_name           = "${local.name_prefix}-rare-handover-bucket-${data.aws_caller_identity.current.account_id}"
  enable_presigned_urls = true
  enable_access_logging = true
  enable_lifecycle      = false
  kms_key_admin_role    = var.role_terraform_arn

  tags = merge(
    var.tags,
    {
      description = "Originally provisioned for receiving database dump files during system migrations. Later repurposed to support rare or ad hoc manual handovers of files and objects between teams or environments."
    }
  )
}
