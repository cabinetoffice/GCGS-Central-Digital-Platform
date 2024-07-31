module "kms" {
  source = "../kms"

  custom_policies          = []
  customer_master_key_spec = "SYMMETRIC_DEFAULT"
  deletion_window_in_days  = 7
  description              = "RDS Managed main password for ${var.db_name}"
  key_admin_role           = var.role_terraform_arn
  key_alias                = "rds-${var.db_name}"
  key_usage                = "ENCRYPT_DECRYPT"
  key_user_arns            = [var.role_cloudwatch_events_arn, var.role_db_connection_step_function_arn]
  other_aws_accounts       = {}
  tags                     = var.tags
}
