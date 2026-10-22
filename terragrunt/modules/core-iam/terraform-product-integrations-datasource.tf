data "aws_iam_policy_document" "terraform_product_integrations" {
  statement {
    actions = [
      "events:CreateConnection",
      "events:DescribeConnection",
      "events:DeleteConnection",
      "events:DeleteRule",
      "events:DescribeRule",
      "events:ListTagsForResource",
      "events:ListTargetsByRule",
      "events:PutRule",
      "events:PutTargets",
      "events:RemoveTargets",
      "events:TagResource",
      "events:UpdateConnection",
      "iam:PassRole",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:events:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:connection/${local.name_prefix}-*",
      "arn:aws:events:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:rule/${local.name_prefix}-*",
      "arn:aws:events:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:rule/StepFunctionsGetEventsForStepFunctionsExecutionRule",
    ]
  }

  statement {
    actions = [
      "states:CreateStateMachine",
      "states:Delete*",
      "states:Describe*",
      "states:List*",
      "states:StartExecution",
      "states:TagResource",
      "states:UpdateStateMachine",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:states:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:stateMachine:${local.name_prefix}-*",
    ]
  }

  statement {
    actions = [
      "states:DescribeExecution",
      "states:GetExecutionHistory",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:states:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:execution:${local.name_prefix}-*:*",
    ]
  }

  statement {
    actions = [
      "scheduler:CreateSchedule",
      "scheduler:UpdateSchedule",
      "scheduler:DeleteSchedule",
      "scheduler:GetSchedule",
      "scheduler:TagResource",
      "scheduler:UntagResource",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:scheduler:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:schedule/*/${local.name_prefix}-*",
    ]
  }

  statement {
    actions = [
      "scheduler:ListSchedules",
      "scheduler:GetScheduleGroup",
      "scheduler:ListScheduleGroups",
    ]
    effect    = "Allow"
    resources = ["*"]
  }

  statement {
    actions = [
      "iam:PassRole",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/${local.name_prefix}-*-scheduler",
    ]
  }

  statement {
    actions = [
      "SNS:CreateTopic",
      "SNS:DeleteTopic",
      "SNS:GetSubscriptionAttributes",
      "SNS:GetTopicAttributes",
      "SNS:ListTagsForResource",
      "SNS:SetTopicAttributes",
      "SNS:Subscribe",
      "SNS:TagResource",
      "SNS:Unsubscribe",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:sns:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:${local.name_prefix}-*",
    ]
  }
}
