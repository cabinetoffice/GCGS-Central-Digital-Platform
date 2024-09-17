data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_vpc_endpoint" "s3" {
  vpc_id       = var.vpc_id
  service_name = "com.amazonaws.eu-west-2.s3"
}

data "aws_iam_policy_document" "canary" {
  statement {
    sid    = "AllowPutCloudwatchMetricData"
    actions = ["cloudwatch:PutMetricData"]
    effect = "Allow"
    resources = ["*"]

    condition {
      test     = "StringEquals"
      variable = "cloudwatch:namespace"
      values = ["CloudWatchSynthetics"]
    }
  }

  statement {
    sid = "AllowWriteLogActivities"
    actions = [
      "logs:CreateLogStream",
      "logs:CreateLogGroup",
      "logs:PutLogEvents"
    ]
    effect = "Allow"
    resources = ["*"]
  }

  statement {
    sid    = "AllowXRay"
    actions = ["xray:PutTraceSegments"]
    effect = "Allow"
    resources = ["*"]
  }

  statement {
    sid = "AllowNetworkInterfaces"
    actions = [
      "ec2:CreateNetworkInterface",
      "ec2:DescribeNetworkInterfaces",
      "ec2:DeleteNetworkInterface"
    ]
    effect = "Allow"
    resources = ["*"]
  }

  statement {
    sid    = "AllowListBucketLocation"
    actions = ["s3:ListAllMyBuckets"]
    effect = "Allow"
    resources = ["*"]
  }

  statement {
    sid    = "AllowGetBucketLocation"
    actions = ["s3:GetBucketLocation"]
    effect = "Allow"
    resources = [module.s3_bucket_canary.arn]
  }

  statement {
    sid = "WriteAccessToS3Bucket"
    actions = [
      "s3:PutObject"
    ]
    resources = [module.s3_bucket_canary.arn]
  }

  statement {
    sid = "GetBasicAuthCredentials"
    actions = [
      "secretsmanager:*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:${local.name_prefix}-canary-dev-credentials-*",
    ]
  }

  statement {
    sid    = "FetchServiceVersionValue"
    effect = "Allow"
    actions = [
      "ssm:GetParameter"
    ]

    resources = [
      "arn:aws:ssm:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:parameter/${local.name_prefix}-service-version"
    ]
  }
}
