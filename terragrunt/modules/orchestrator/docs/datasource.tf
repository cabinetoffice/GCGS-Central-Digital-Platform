data "aws_region" "current" {}

data "aws_iam_policy_document" "docs_publisher" {
  statement {
    sid     = "DocsBucketList"
    effect  = "Allow"
    actions = ["s3:ListBucket"]
    resources = [
      module.docs_bucket.arn
    ]
  }

  statement {
    sid     = "DocsBucketWrite"
    effect  = "Allow"
    actions = ["s3:GetObject", "s3:PutObject", "s3:DeleteObject"]
    resources = [
      "${module.docs_bucket.arn}/*"
    ]
  }
}

data "aws_iam_policy_document" "github_oidc_assume" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRoleWithWebIdentity"]

    principals {
      type        = "Federated"
      identifiers = [aws_iam_openid_connect_provider.github.arn]
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
          "repo:${var.github_org}/${var.github_repo}:ref:refs/heads/${branch}"
        ],
        var.allow_github_pull_requests ? [
          "repo:${var.github_org}/${var.github_repo}:pull_request",
          "repo:${var.github_org}/${var.github_repo}:ref:refs/pull/*/merge"
        ] : []
      ) : ["repo:${var.github_org}/${var.github_repo}:*"]
    }
  }
}
