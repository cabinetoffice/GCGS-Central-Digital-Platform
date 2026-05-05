data "aws_iam_policy_document" "origin_bucket" {
  count = var.cloudfront_manage_origin_bucket ? 1 : 0
  statement {
    sid    = "EnforceSSL"
    effect = "Deny"

    principals {
      type        = "*"
      identifiers = ["*"]
    }

    actions = ["s3:*"]

    resources = [
      local.origin_bucket_arn,
      "${local.origin_bucket_arn}/*"
    ]

    condition {
      test     = "Bool"
      variable = "aws:SecureTransport"
      values   = ["false"]
    }
  }

  dynamic "statement" {
    for_each = var.cloudfront_enabled ? [1] : []
    content {
      sid    = "AllowCloudFrontRead"
      effect = "Allow"

      principals {
        type        = "Service"
        identifiers = ["cloudfront.amazonaws.com"]
      }

      actions = ["s3:GetObject"]

      resources = ["${local.origin_bucket_arn}/*"]

      condition {
        test     = "StringEquals"
        variable = "AWS:SourceArn"
        values   = [aws_cloudfront_distribution.this[0].arn]
      }

      condition {
        test     = "StringEquals"
        variable = "AWS:SourceAccount"
        values   = [data.aws_caller_identity.current.account_id]
      }
    }
  }
}

data "aws_iam_policy_document" "log_bucket" {
  statement {
    sid    = "EnforceSSL"
    effect = "Deny"

    principals {
      type        = "*"
      identifiers = ["*"]
    }

    actions = ["s3:*"]

    resources = [
      aws_s3_bucket.logs.arn,
      "${aws_s3_bucket.logs.arn}/*"
    ]

    condition {
      test     = "Bool"
      variable = "aws:SecureTransport"
      values   = ["false"]
    }
  }

  dynamic "statement" {
    for_each = var.cloudfront_enabled ? [1] : []
    content {
      sid    = "AllowCloudFrontLogDeliveryAcl"
      effect = "Allow"

      principals {
        type        = "Service"
        identifiers = ["cloudfront.amazonaws.com"]
      }

      actions   = ["s3:GetBucketAcl"]
      resources = [aws_s3_bucket.logs.arn]

      condition {
        test     = "StringEquals"
        variable = "AWS:SourceArn"
        values   = [aws_cloudfront_distribution.this[0].arn]
      }

      condition {
        test     = "StringEquals"
        variable = "AWS:SourceAccount"
        values   = [data.aws_caller_identity.current.account_id]
      }
    }
  }

  dynamic "statement" {
    for_each = var.cloudfront_enabled ? [1] : []
    content {
      sid    = "AllowCloudFrontLogDeliveryWrite"
      effect = "Allow"

      principals {
        type        = "Service"
        identifiers = ["cloudfront.amazonaws.com"]
      }

      actions   = ["s3:PutObject"]
      resources = ["${aws_s3_bucket.logs.arn}/*"]

      condition {
        test     = "StringEquals"
        variable = "AWS:SourceArn"
        values   = [aws_cloudfront_distribution.this[0].arn]
      }

      condition {
        test     = "StringEquals"
        variable = "AWS:SourceAccount"
        values   = [data.aws_caller_identity.current.account_id]
      }
    }
  }
}
