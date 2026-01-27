data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_subnet" "first_public_subnet" {
  id = var.public_subnet_ids[0]
}

data "aws_secretsmanager_secret_version" "allowed_ips" {
  secret_id = "cdp-sirsi-waf-allowed-ip-set-tools"
}

data "aws_iam_policy_document" "db_import_handover_s3" {
  statement {
    sid    = "ListBucket"
    effect = "Allow"
    actions = [
      "s3:ListBucket",
      "s3:GetBucketLocation",
      "s3:GetEncryptionConfiguration",
    ]
    resources = [
      "arn:aws:s3:::${module.sql_dump_upload_bucket.bucket}"
    ]
  }

  statement {
    sid    = "WriteObjects"
    effect = "Allow"
    actions = [
      "s3:PutObject",
      "s3:AbortMultipartUpload",
      "s3:ListBucketMultipartUploads",
      "s3:ListMultipartUploadParts"
    ]
    resources = [
      "arn:aws:s3:::${module.sql_dump_upload_bucket.bucket}/*"
    ]
  }

  statement {
    sid    = "ReadBackOptional"
    effect = "Allow"
    actions = [
      "s3:GetObject",
      "s3:GetObjectVersion"
    ]
    resources = [
      "arn:aws:s3:::${module.sql_dump_upload_bucket.bucket}/*"
    ]
  }

  statement {
    sid    = "AllowDbImportRoleUseOfKeyForS3"
    effect = "Allow"
    actions = [
      "kms:Decrypt",
      "kms:Encrypt",
      "kms:GenerateDataKey",
    ]
    resources = [module.sql_dump_upload_bucket.key_arn]
  }
}
