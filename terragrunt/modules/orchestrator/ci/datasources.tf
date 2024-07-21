data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_iam_policy_document" "orchestrator_pipeline" {

  statement {

    actions = [
      "codepipeline:StartPipelineExecution"
    ]
    effect    = "Allow"
    resources = ["*"]
  }
}

data "aws_iam_policy_document" "orchestrator_codebuild" {

  statement {

    actions = [
      "sts:AssumeRole"
    ]
    effect    = "Allow"
    resources = [for name, id in var.account_ids : "arn:aws:iam::${id}:role/cdp-sirsi-${name}-terraform"]
  }
}
