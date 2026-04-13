resource "aws_iam_role" "cloudfront_realtime_logs" {
  name               = "${local.name_prefix}-cloudfront-rt-logs"
  assume_role_policy = data.aws_iam_policy_document.cloudfront_realtime_logs_assume.json
}

resource "aws_iam_role_policy" "cloudfront_realtime_logs" {
  name   = "${local.name_prefix}-cloudfront-rt-logs"
  role   = aws_iam_role.cloudfront_realtime_logs.id
  policy = data.aws_iam_policy_document.cloudfront_realtime_logs.json
}
