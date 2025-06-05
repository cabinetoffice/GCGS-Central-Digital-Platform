module "s3_kms_key" {
  source = "../kms"

  bucket_enable_presigned  = var.enable_presigned_urls
  bucket_name              = var.bucket_name
  custom_policies          = []
  customer_master_key_spec = "SYMMETRIC_DEFAULT"
  deletion_window_in_days  = 7
  description              = var.kms_key_description
  key_admin_role           = var.kms_key_admin_role
  key_alias                = "buckets-${var.bucket_name}"
  key_usage                = "ENCRYPT_DECRYPT"
  key_user_arns            = concat(var.read_roles, var.write_roles)
  other_aws_accounts       = {}
  tags                     = var.tags
}
