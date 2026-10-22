data "aws_iam_policy_document" "terraform_product" {

  statement {
    actions = [
      "iam:*",
      "ecs:*"
    ]
    effect = "Allow"
    resources = [
      "arn:aws:iam::${data.aws_caller_identity.current.account_id}:instance-profile/${local.name_prefix}-*",
      "arn:aws:iam::${data.aws_caller_identity.current.account_id}:policy/${local.name_prefix}-*",
      "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/${local.name_prefix}-*",
      "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/aws-service-role/*",
      "arn:aws:iam::${data.aws_caller_identity.current.account_id}:user/${local.name_prefix}-*",
    ]
  }

  statement {
    actions = [
      "s3:GetObject",
      "s3:GetObjectVersion"
    ]
    effect = "Allow"
    resources = [
      "arn:aws:s3:::aws-synthetics-library-eu-west-2/*"
    ]
  }

  statement {
    actions = [
      "lambda:Add*",
      "lambda:Create*",
      "lambda:Get*",
      "lambda:Publish*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:lambda:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:function:cwsyn-${local.name_prefix}-*",
      "arn:aws:lambda:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:layer:cwsyn-${local.name_prefix}-*",
      "arn:aws:lambda:${data.aws_region.current.region}:*:layer:Synthetics_*",
    ]
  }

  statement {
    actions = [
      "dynamodb:*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:dynamodb:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:table/${local.name_prefix}-*",
    ]
  }

  statement {
    actions = [
      "synthetics:Create*",
      "synthetics:DeleteCanary",
      "synthetics:Get*",
      "synthetics:Start*",
      "synthetics:Stop*",
      "synthetics:Tag*",
      "synthetics:Update*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:synthetics:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:canary:${local.name_prefix}-*"
    ]
  }

}
