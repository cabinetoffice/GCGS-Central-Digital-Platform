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

data "aws_secretsmanager_secret" "fts_app_secrets" {
  name = "${local.name_prefix}-fts/secrets"
}

data "aws_secretsmanager_secret_version" "fts_app_secrets" {
  secret_id = data.aws_secretsmanager_secret.fts_app_secrets.id
}

data "aws_secretsmanager_secret" "cfs_app_secrets" {
  name = "${local.name_prefix}-cfs/secrets"
}

data "aws_secretsmanager_secret_version" "cfs_app_secrets" {
  secret_id = data.aws_secretsmanager_secret.cfs_app_secrets.id
}

data "aws_iam_policy_document" "rds_proxy" {
  statement {
    sid    = "ReadDbSecrets"
    effect = "Allow"
    actions = [
      "secretsmanager:DescribeSecret",
      "secretsmanager:GetSecretValue",
    ]
    resources = concat(
      [
        module.cluster_fts.db_credentials_arn,
        module.cluster_cfs.db_credentials_arn,
        "arn:aws:secretsmanager:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:secret:${local.name_prefix}-fts-rds-proxy-*-credentials*",
        "arn:aws:secretsmanager:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:secret:${local.name_prefix}-cfs-rds-proxy-*-credentials*",
      ],
    )
  }

  statement {
    sid    = "DecryptSecretKms"
    effect = "Allow"
    actions = [
      "kms:Decrypt",
    ]
    resources = ["*"]

    condition {
      test     = "StringEquals"
      variable = "kms:ViaService"
      values   = ["secretsmanager.${data.aws_region.current.region}.amazonaws.com"]
    }

    condition {
      test     = "StringEquals"
      variable = "kms:CallerAccount"
      values   = [data.aws_caller_identity.current.account_id]
    }
  }
}
