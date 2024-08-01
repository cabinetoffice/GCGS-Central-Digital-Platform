data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_iam_policy_document" "access_policy" {

  statement {
    sid    = "AllowAccessToQueue"
    effect = "Allow"
    principals {
      identifiers = concat(var.role_consumer_arn, var.role_publisher_arn)
      type        = "AWS"
    }
    actions = [
      "sqs:DeleteMessage",
      "sqs:GetQueueAttributes",
      "sqs:GetQueueUrl",
      "sqs:ReceiveMessage",
      "sqs:SendMessage"
    ]
    resources = [local.arn]
  }

  statement {
    sid    = "AllowAccessToDLQ"
    effect = "Allow"
    principals {
      identifiers = concat(var.role_consumer_arn, var.role_publisher_arn)
      type        = "AWS"
    }
    actions = [
      "sqs:DeleteMessage",
      "sqs:GetQueueAttributes",
      "sqs:GetQueueUrl",
      "sqs:ReceiveMessage",
      "sqs:SendMessage"
    ]
    resources = [local.dlq_arn]
  }

  statement {
    sid    = "AllowConsumeFromQueue"
    effect = "Allow"
    principals {
      identifiers = var.role_consumer_arn
      type        = "AWS"
    }
    actions = [
      "sqs:DeleteMessage",
      "sqs:ReceiveMessage",
    ]
    resources = [local.arn]
  }

  statement {
    sid    = "AllowConsumeFromDLQ"
    effect = "Allow"
    principals {
      identifiers = var.role_consumer_arn
      type        = "AWS"
    }
    actions = [
      "sqs:DeleteMessage",
      "sqs:ReceiveMessage",
    ]
    resources = [local.dlq_arn]
  }

  statement {
    sid    = "AllowPublishToQueue"
    effect = "Allow"
    principals {
      identifiers = var.role_publisher_arn
      type        = "AWS"
    }
    actions = [
      "sqs:ReceiveMessage",
    ]
    resources = [local.arn]
  }

}
