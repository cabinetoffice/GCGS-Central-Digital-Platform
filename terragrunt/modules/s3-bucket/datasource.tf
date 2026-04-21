data "aws_caller_identity" "current" {}

data "aws_iam_policy_document" "private" {

  statement {
    sid    = "EnforceSSL"
    effect = "Deny"

    principals {
      type        = "*"
      identifiers = ["*"]
    }

    actions = ["s3:*"]

    resources = [
      aws_s3_bucket.this.arn,
      "${aws_s3_bucket.this.arn}/*"
    ]

    condition {
      test     = "Bool"
      variable = "aws:SecureTransport"
      values   = ["false"]
    }
  }

  dynamic "statement" {
    for_each = length(var.write_roles) > 0 ? [var.write_roles] : []
    content {
      sid    = "WriteAccess"
      effect = "Allow"
      principals {
        type        = "AWS"
        identifiers = statement.value
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
        aws_s3_bucket.this.arn,
        "${aws_s3_bucket.this.arn}/*"
      ]
    }
  }

  dynamic "statement" {
    for_each = length(var.read_roles) > 0 ? [var.read_roles] : []
    content {
      sid    = "ReadAccess"
      effect = "Allow"
      principals {
        type        = "AWS"
        identifiers = statement.value
      }
      actions = [
        "s3:GetObject",
        "s3:ListBucket"
      ]
      resources = [
        aws_s3_bucket.this.arn,
        "${aws_s3_bucket.this.arn}/*"
      ]
    }
  }

  dynamic "statement" {
    for_each = length(var.write_roles) > 0 ? [var.write_roles] : []
    content {
      sid    = "WriteObjectOnly"
      effect = "Allow"
      principals {
        type        = "AWS"
        identifiers = statement.value
      }
      actions   = ["s3:PutObject"]
      resources = ["${aws_s3_bucket.this.arn}/*"]
    }
  }
}

data "aws_iam_policy_document" "public" {
  statement {
    sid       = "PublicAccess"
    actions   = ["s3:GetObject"]
    resources = ["${aws_s3_bucket.this.arn}/*"]
    principals {
      type        = "*"
      identifiers = ["*"]
    }
  }

  dynamic "statement" {
    for_each = length(var.write_roles) > 0 ? [var.write_roles] : []
    content {
      sid    = "WriteAccess"
      effect = "Allow"
      actions = [
        "s3:PutObjectAcl",
        "s3:PutObject",
        "s3:ListBucket",
        "s3:GetObjectVersion",
        "s3:GetObject",
        "s3:GetBucketVersioning",
        "s3:DeleteObject"
      ]
      resources = [
        "${aws_s3_bucket.this.arn}",
        "${aws_s3_bucket.this.arn}/*"
      ]
      principals {
        type        = "AWS"
        identifiers = statement.value
      }
    }
  }
}
