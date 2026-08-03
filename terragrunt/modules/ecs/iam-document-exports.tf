data "aws_iam_policy_document" "document_exports_external_read" {
  statement {
    sid     = "ListExportsPrefix"
    effect  = "Allow"
    actions = ["s3:ListBucket"]

    resources = [module.s3_bucket_document_exports.arn]

    condition {
      test     = "StringLike"
      variable = "s3:prefix"
      values = [
        var.document_exports_prefix,
        "${var.document_exports_prefix}*",
      ]
    }
  }

  statement {
    sid     = "ReadExportsObjects"
    effect  = "Allow"
    actions = ["s3:GetObject", "s3:GetObjectVersion"]

    resources = ["${module.s3_bucket_document_exports.arn}/${var.document_exports_prefix}*"]
  }
}

resource "aws_iam_policy" "document_exports_external_read" {
  name   = "${local.name_prefix}-document-exports-external-read"
  policy = data.aws_iam_policy_document.document_exports_external_read.json
  tags   = var.tags
}

resource "aws_iam_user" "document_exports_reader" {
  count = var.document_exports_external_access_key_enabled ? 1 : 0
  name  = "${local.name_prefix}-document-exports-reader"
  tags  = var.tags
}

resource "aws_iam_user_policy_attachment" "document_exports_reader" {
  count      = var.document_exports_external_access_key_enabled ? 1 : 0
  user       = aws_iam_user.document_exports_reader[0].name
  policy_arn = aws_iam_policy.document_exports_external_read.arn
}

resource "aws_iam_access_key" "document_exports_reader" {
  count = var.document_exports_external_access_key_enabled ? 1 : 0
  user  = aws_iam_user.document_exports_reader[0].name
}

resource "aws_secretsmanager_secret" "document_exports_reader_credentials" {
  count       = var.document_exports_external_access_key_enabled ? 1 : 0
  name        = "${local.name_prefix}-document-exports-reader-credentials"
  description = "Credentials for external consumers to read exported documents from S3"
  tags        = var.tags
}

resource "aws_secretsmanager_secret_version" "document_exports_reader_credentials" {
  count     = var.document_exports_external_access_key_enabled ? 1 : 0
  secret_id = aws_secretsmanager_secret.document_exports_reader_credentials[0].id

  secret_string = jsonencode({
    AWS_ACCESS_KEY_ID     = aws_iam_access_key.document_exports_reader[0].id
    AWS_SECRET_ACCESS_KEY = aws_iam_access_key.document_exports_reader[0].secret
    BUCKET_NAME           = module.s3_bucket_document_exports.bucket
    PREFIX                = var.document_exports_prefix
    AWS_REGION            = data.aws_region.current.region
  })
}

# Optional: allow an external OIDC (e.g. Azure workload identity)
data "aws_iam_policy_document" "document_exports_oidc_assume_role" {
  count = var.document_exports_oidc_enabled ? 1 : 0

  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRoleWithWebIdentity"]

    principals {
      type        = "Federated"
      identifiers = [var.document_exports_oidc_provider_arn]
    }

    condition {
      test     = "StringEquals"
      variable = "${var.document_exports_oidc_provider_url}:aud"
      values   = var.document_exports_oidc_audiences
    }

    condition {
      test     = "StringLike"
      variable = "${var.document_exports_oidc_provider_url}:sub"
      values   = var.document_exports_oidc_subjects
    }
  }
}

resource "aws_iam_role" "document_exports_reader_oidc" {
  count              = var.document_exports_oidc_enabled ? 1 : 0
  name               = "${local.name_prefix}-document-exports-reader-oidc"
  assume_role_policy = data.aws_iam_policy_document.document_exports_oidc_assume_role[0].json
  tags               = var.tags
}

resource "aws_iam_role_policy_attachment" "document_exports_reader_oidc" {
  count      = var.document_exports_oidc_enabled ? 1 : 0
  role       = aws_iam_role.document_exports_reader_oidc[0].name
  policy_arn = aws_iam_policy.document_exports_external_read.arn
}
