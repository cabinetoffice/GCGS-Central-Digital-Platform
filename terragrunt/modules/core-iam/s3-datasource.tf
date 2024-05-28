data "aws_s3_bucket" "tfstate" {
  bucket = var.tfstate_bucket_name
}

data "aws_iam_policy_document" "tfstate" {
  statement {
    sid    = "EnforcedTLS"
    effect = "Deny"

    principals {
      type        = "AWS"
      identifiers = ["*"]
    }

    actions = ["s3:*"]

    resources = [
      "arn:aws:s3:::tfstate-cdp-sirsi-staging-${data.aws_caller_identity.current.account_id}",
      "arn:aws:s3:::tfstate-cdp-sirsi-staging-${data.aws_caller_identity.current.account_id}/*",
    ]

    condition {
      test     = "Bool"
      variable = "aws:SecureTransport"
      values   = ["false"]
    }
  }

  statement {
    sid    = "RootAccess"
    effect = "Allow"

    principals {
      type = "AWS"
      identifiers = [
        "arn:aws:iam::${data.aws_caller_identity.current.account_id}:root",
        aws_iam_role.terraform.arn
      ]
    }

    actions = ["s3:*"]

    resources = [
      "arn:aws:s3:::tfstate-cdp-sirsi-staging-${data.aws_caller_identity.current.account_id}",
      "arn:aws:s3:::tfstate-cdp-sirsi-staging-${data.aws_caller_identity.current.account_id}/*",
    ]
  }
}
