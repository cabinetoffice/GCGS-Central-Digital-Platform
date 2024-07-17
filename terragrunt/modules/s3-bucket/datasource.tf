data "aws_caller_identity" "current" {}

data "aws_iam_policy_document" "bucket" {
  statement {
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

  statement {
    sid    = "PublicAccess"
    effect = "Allow"
    principals {
      type        = "AWS"
      identifiers = ["*"]
    }
    actions = [
      "s3:GetObject",
    ]
    resources = [
      aws_s3_bucket.bucket.arn,
      "${aws_s3_bucket.bucket.arn}/*"
    ]
  }
}
