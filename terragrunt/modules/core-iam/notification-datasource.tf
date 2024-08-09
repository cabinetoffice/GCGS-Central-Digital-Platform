data "aws_iam_policy_document" "notification_step_function_assume" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["states.amazonaws.com"]
    }
    condition {
      test     = "StringEquals"
      values = [local.orchestrator_account_id]
      variable = "aws:SourceAccount"
    }
    condition {
      test     = "ArnLike"
      variable = "aws:SourceArn"
      values   = ["arn:aws:states:${data.aws_region.current.name}:${local.orchestrator_account_id}:stateMachine:*"]
    }
  }
}
