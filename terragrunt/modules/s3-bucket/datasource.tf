data "aws_caller_identity" "current" {}

data "aws_iam_policy_document" "bucket" {

  statement {
    sid    = "EnforceSSL"
    effect = "Deny"

    principals {
      type        = "*"
      identifiers = ["*"]
    }

    actions = ["s3:*"]

    resources = [
      aws_s3_bucket.bucket.arn,
      "${aws_s3_bucket.bucket.arn}/*"
    ]

    condition {
      test     = "Bool"
      variable = "aws:SecureTransport"
      values   = ["false"]
    }
  }

  dynamic "statement" {
    for_each = length(local.all_roles) > 0 ? [local.all_roles] : []
    content {
      sid    = "ApplicationAccess"
      effect = "Allow"
      principals {
        type        = "AWS"
        identifiers = local.all_roles
      }
      actions = [
        "s3:DeleteObject",
        "s3:GetObject",
        "s3:GetObjectVersion",
        "s3:GetBucketVersioning",
        "s3:ListBucket",
        "s3:PutObjectAcl",
        "s3:PutObject",
      ]
      resources = [
        aws_s3_bucket.bucket.arn,
        "${aws_s3_bucket.bucket.arn}/*"
      ]
    }
  }

  dynamic "statement" {
    for_each = length(var.read_roles) > 0 ? var.read_roles : []
    content {
      sid    = "OptionalReadAccess"
      effect = "Allow"
      principals {
        type        = "AWS"
        identifiers = [statement.value]
      }
      actions = ["s3:GetObject"]
      resources = ["${aws_s3_bucket.bucket.arn}/*"]
    }
  }

  dynamic "statement" {
    for_each = length(var.write_roles) > 0 ? var.write_roles : []
    content {
      sid    = "OptionalWriteAccess"
      effect = "Allow"
      principals {
        type        = "AWS"
        identifiers = [statement.value]
      }
      actions = ["s3:PutObject"]
      resources = ["${aws_s3_bucket.bucket.arn}/*"]
    }
  }
}

