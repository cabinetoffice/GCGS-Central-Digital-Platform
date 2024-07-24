module "s3_kms_key" {
  source = "../kms"

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
