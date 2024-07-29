data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_iam_policy_document" "inbound_queue_policy" {
  statement {
    sid    = "AllowAccessToTaskRole"
    effect = "Allow"
    principals {
      identifiers = [var.role_ecs_task_arn]
      type        = "AWS"
    }
    actions = [
      "sqs:DeleteMessage",
      "sqs:GetQueueAttributes",
      "sqs:GetQueueUrl",
      "sqs:ReceiveMessage",
      "sqs:SendMessage"
    ]
    resources = ["arn:aws:sqs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:${local.name_entity_verification_queue}"]
  }
}

data "aws_iam_policy_document" "outbound_queue_policy" {
  statement {
    sid    = "AllowAccessToTaskRole"
    effect = "Allow"
    principals {
      identifiers = [var.role_ecs_task_arn]
      type        = "AWS"
    }
    actions = [
      "sqs:DeleteMessage",
      "sqs:GetQueueAttributes",
      "sqs:GetQueueUrl",
      "sqs:ReceiveMessage",
      "sqs:SendMessage"
    ]
    resources = ["arn:aws:sqs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:${local.name_entity_verification_queue}"]
  }
}