data "aws_iam_policy_document" "canary_assume" {
  statement {
    sid = "1"
    principals {
      identifiers = ["lambda.amazonaws.com"]
      type        = "Service"
    }

    actions = ["sts:AssumeRole"]
  }
}
