import {
  id = ""
  to = "module.rds_entity_verification.data.aws_secretsmanager_secret.this"
}
module "kms" {
  source = "../kms"

  custom_policies          = []
  customer_master_key_spec = "SYMMETRIC_DEFAULT"
  deletion_window_in_days  = 7
  description              = "RDS Managed main password for ${var.db_name}"
  key_admin_role           = var.role_terraform_arn
  key_alias                = "rds-${var.db_name}"
  key_usage                = "ENCRYPT_DECRYPT"
  key_user_arns            = []
  other_aws_accounts       = {}
  tags                     = var.tags
}
