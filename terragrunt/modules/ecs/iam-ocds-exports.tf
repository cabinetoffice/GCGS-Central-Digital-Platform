locals {
  ocds_exports_reader_oidc_role_name = "${local.name_prefix}-ocds-exports-reader-oidc"
  ocds_exports_reader_oidc_role_arn  = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/${local.ocds_exports_reader_oidc_role_name}"
}

data "aws_iam_policy_document" "ocds_exports_external_read" {
  statement {
    sid     = "GetExportsBucketLocation"
    effect  = "Allow"
    actions = ["s3:GetBucketLocation"]

    resources = [module.s3_bucket_ocds_exports.arn]
  }

  statement {
    sid     = "GetExportsBucketVersioning"
    effect  = "Allow"
    actions = ["s3:GetBucketVersioning"]

    resources = [module.s3_bucket_ocds_exports.arn]
  }

  statement {
    sid     = "ListExportsPrefix"
    effect  = "Allow"
    actions = ["s3:ListBucket"]

    resources = [module.s3_bucket_ocds_exports.arn]

    condition {
      test     = "StringLike"
      variable = "s3:prefix"
      values = [
        # Some clients use prefix=exports (no trailing slash). Allow both forms.
        trimsuffix(var.ocds_exports_prefix, "/"),
        var.ocds_exports_prefix,
        "${var.ocds_exports_prefix}*",
      ]
    }
  }

  # Some S3 clients issue ListBucket without a prefix during validation/browsing.
  statement {
    sid     = "ListExportsNoPrefix"
    effect  = "Allow"
    actions = ["s3:ListBucket"]

    resources = [module.s3_bucket_ocds_exports.arn]

    condition {
      test     = "Null"
      variable = "s3:prefix"
      values   = ["true"]
    }
  }

  statement {
    sid     = "ReadExportsObjects"
    effect  = "Allow"
    actions = [
      "s3:GetObject",
      "s3:GetObjectAttributes",
      "s3:GetObjectTagging",
      "s3:GetObjectVersion"
    ]

    resources = ["${module.s3_bucket_ocds_exports.arn}/${var.ocds_exports_prefix}*"]
  }

  dynamic "statement" {
    for_each = var.unity_catalog_assume_role_principal_arn != null ? [1] : []
    content {
      sid     = "AllowSelfAssumeRole"
      effect  = "Allow"
      actions = ["sts:AssumeRole"]

      resources = [local.ocds_exports_reader_oidc_role_arn]
    }
  }
}

resource "aws_iam_policy" "ocds_exports_external_read" {
  name   = "${local.name_prefix}-ocds-exports-external-read"
  policy = data.aws_iam_policy_document.ocds_exports_external_read.json
  tags   = var.tags
}

resource "aws_iam_openid_connect_provider" "ocds_exports" {
  url            = local.ocds_exports_oidc_provider_url
  client_id_list = [local.ocds_exports_oidc_client_id]

  # For Microsoft Entra issuers under sts.windows.net AWS documents using an all-zero placeholder thumbprint.
  # AWS validates tokens against the issuer JWKS endpoint.
  thumbprint_list = ["0000000000000000000000000000000000000000"]

  tags = var.tags
}

data "aws_iam_policy_document" "ocds_exports_oidc_assume_role" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRoleWithWebIdentity"]

    principals {
      type        = "Federated"
      identifiers = [aws_iam_openid_connect_provider.ocds_exports.arn]
    }

    # TODO(security): tighten trust policy once Databricks is validated end-to-end.
    # Add conditions for at least:
    # - aud == CLIENT_ID
    # - tid == TENANT_ID
    # And ideally also:
    # - oid (service principal object id) and/or sub patterns
  }

  dynamic "statement" {
    for_each = var.unity_catalog_assume_role_principal_arn != null ? [1] : []
    content {
      sid     = "AllowUnityCatalogAssumeRole"
      effect  = "Allow"
      actions = ["sts:AssumeRole"]

      principals {
        type        = "AWS"
        identifiers = [var.unity_catalog_assume_role_principal_arn]
      }

      condition {
        test     = "StringEquals"
        variable = "sts:ExternalId"
        values   = [var.unity_catalog_assume_role_external_id]
      }
    }
  }

  dynamic "statement" {
    for_each = var.unity_catalog_assume_role_principal_arn != null ? [1] : []
    content {
      sid     = "AllowSelfAssumeRole"
      effect  = "Allow"
      actions = ["sts:AssumeRole"]

      principals {
        type        = "AWS"
        identifiers = [local.ocds_exports_reader_oidc_role_arn]
      }
    }
  }
}

resource "aws_iam_role" "ocds_exports_reader_oidc" {
  name               = local.ocds_exports_reader_oidc_role_name
  assume_role_policy = data.aws_iam_policy_document.ocds_exports_oidc_assume_role.json
  tags               = var.tags
}

resource "aws_iam_role_policy_attachment" "ocds_exports_reader_oidc" {
  role       = aws_iam_role.ocds_exports_reader_oidc.name
  policy_arn = aws_iam_policy.ocds_exports_external_read.arn
}
