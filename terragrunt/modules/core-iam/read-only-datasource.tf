data "aws_iam_policy_document" "read_only_assume" {
  statement {
    sid     = "AllowReadOnlyOperatorsAssumeRole"
    actions = ["sts:AssumeRole"]
    principals {
      type        = "AWS"
      identifiers = local.read_only_operators
    }
    condition {
      test     = "Bool"
      variable = "aws:MultiFactorAuthPresent"
      values   = [true]
    }
  }
}

