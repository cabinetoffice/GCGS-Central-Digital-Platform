# Note!
# Resources in this file are shared with orchestrator/iam module

data "aws_iam_policy_document" "terraform_assume" {
  statement {
    sid     = "AllowTerraformOperatorsAssumeRole"
    actions = ["sts:AssumeRole"]
    principals {
      type        = "AWS"
      identifiers = var.terraform_operators
    }
    condition {
      test     = "Bool"
      values   = [true]
      variable = "aws:MultiFactorAuthPresent"
    }
  }

  statement {
    sid     = "AllowOrchestratorCodebuildAssumeRole"
    actions = ["sts:AssumeRole"]
    principals {
      type        = "AWS"
      identifiers = ["arn:aws:iam::${local.orchestrator_account_id}:role/${local.name_prefix}-orchestrator-ci-codebuild"]
    }
  }
}

data "aws_iam_policy_document" "terraform_assume_orchestrator_role" {
  statement {
    effect    = "Allow"
    actions   = ["sts:AssumeRole"]
    resources = ["arn:aws:iam::${local.orchestrator_account_id}:role/${local.name_prefix}-orchestrator-read-service-version"]
  }
}

data "aws_iam_policy_document" "terraform" {

  statement {
    actions = ["dynamodb:*"]
    effect  = "Allow"
    resources = [
      "arn:aws:dynamodb:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:table/terraform-locks"
    ]
    sid = "ManageTerraformLock"
  }

}
