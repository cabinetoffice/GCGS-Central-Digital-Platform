data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_codestarconnections_connection" "cabinet_office" {
  name = "CabinetOffice"
}

data "aws_iam_openid_connect_provider" "github" {
  url = "https://token.actions.githubusercontent.com"
}

data "aws_iam_policy_document" "github_oidc_assume" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRoleWithWebIdentity"]

    principals {
      type        = "Federated"
      identifiers = [data.aws_iam_openid_connect_provider.github.arn]
    }

    condition {
      test     = "StringEquals"
      variable = "token.actions.githubusercontent.com:aud"
      values   = ["sts.amazonaws.com"]
    }

    condition {
      test     = "StringLike"
      variable = "token.actions.githubusercontent.com:sub"
      values = length(var.allowed_github_branches) > 0 ? concat(
        [
          for branch in var.allowed_github_branches :
          "repo:${var.github_org}/${var.fts_github_repo}:ref:refs/heads/${branch}"
        ],
        var.allow_github_pull_requests ? [
          "repo:${var.github_org}/${var.fts_github_repo}:pull_request",
          "repo:${var.github_org}/${var.fts_github_repo}:ref:refs/pull/*/merge"
        ] : []
      ) : ["repo:${var.github_org}/${var.fts_github_repo}:*"]
    }
  }
}

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
    resources = [for name, id in var.account_ids : "arn:aws:iam::${id}:role/${local.name_prefix}-${name}-terraform"]
  }
}

data "aws_iam_policy_document" "cloudwatch_events_policy" {
  statement {
    effect = "Allow"

    actions = ["codepipeline:StartPipelineExecution"]

    resources = [
      aws_codepipeline.this.arn
    ]
  }
}

data "aws_iam_policy_document" "fts_db_backup_read" {
  statement {
    effect  = "Allow"
    actions = ["s3:ListBucket"]
    resources = [
      module.fts_db_backup_bucket.arn
    ]
  }

  statement {
    effect  = "Allow"
    actions = ["s3:GetObject"]
    resources = [
      "${module.fts_db_backup_bucket.arn}/*"
    ]
  }
}
