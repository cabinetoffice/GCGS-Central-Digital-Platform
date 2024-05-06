resource "aws_kms_alias" "rds" {
  name          = "alias/${local.name_prefix}-rds-main-password"
  target_key_id = aws_kms_key.rds.id
}

resource "aws_kms_key" "rds" {
  description = "RDS Managed main password for ${local.name_prefix}"
  tags        = var.tags
}
