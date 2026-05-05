data "aws_iam_policy_document" "cloudfront_realtime_logs_assume" {
  statement {
    effect = "Allow"
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["cloudfront.amazonaws.com"]
    }
  }
}

data "aws_iam_policy_document" "cloudfront_realtime_logs" {
  statement {
    effect = "Allow"
    actions = [
      "kinesis:PutRecord",
      "kinesis:PutRecords"
    ]
    resources = [
      "arn:aws:kinesis:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:stream/${local.name_prefix}-*-rt-logs*"
    ]
  }
}
