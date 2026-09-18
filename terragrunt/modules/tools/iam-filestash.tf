data "aws_iam_policy_document" "filestash_s3_readonly" {
  statement {
    sid    = "ListBucket"
    effect = "Allow"
    actions = [
      "s3:ListBucket",
    ]
    resources = [
      local.filestash_reports_bucket_arn,
    ]
  }

  statement {
    sid    = "GetObject"
    effect = "Allow"
    actions = [
      "s3:GetObject",
    ]
    resources = [
      "${local.filestash_reports_bucket_arn}/*",
    ]
  }

  dynamic "statement" {
    for_each = local.filestash_reports_bucket_kms_key_arn == null ? [] : [1]
    content {
      sid    = "DecryptKmsForS3"
      effect = "Allow"
      actions = [
        "kms:Decrypt",
      ]
      resources = [
        local.filestash_reports_bucket_kms_key_arn,
      ]

      condition {
        test     = "StringEquals"
        variable = "kms:ViaService"
        values   = ["s3.${data.aws_region.current.region}.amazonaws.com"]
      }

      condition {
        test     = "StringLike"
        variable = "kms:EncryptionContext:aws:s3:arn"
        values   = ["${local.filestash_reports_bucket_arn}/*"]
      }
    }
  }
}

resource "aws_iam_policy" "filestash_s3_readonly" {
  count = var.filestash_config != null ? 1 : 0

  name        = "${local.name_prefix}-${var.filestash_config.name}-s3-readonly"
  description = "Read-only access for Filestash to browse the E2E nightly reports bucket"
  policy      = data.aws_iam_policy_document.filestash_s3_readonly.json
  tags        = var.tags
}

resource "aws_iam_role_policy_attachment" "filestash_s3_readonly" {
  count = var.filestash_config != null ? 1 : 0

  role       = var.role_filestash_task_name
  policy_arn = aws_iam_policy.filestash_s3_readonly[0].arn
}
