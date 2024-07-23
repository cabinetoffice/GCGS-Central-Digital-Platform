resource "aws_kms_key" "ci_bucket" {
  description             = "KMS key to use for the artefacts"
  deletion_window_in_days = 7
  tags                    = var.tags
}

resource "aws_kms_alias" "pipeline_bucket" {
  name          = "alias/${local.name_prefix}-ci-bucket"
  target_key_id = aws_kms_key.ci_bucket.id
}
