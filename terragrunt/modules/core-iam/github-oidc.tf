resource "aws_iam_openid_connect_provider" "github_actions" {
  url = "https://token.actions.githubusercontent.com"

  client_id_list = ["sts.amazonaws.com"]

  # AWS validates GitHub Actions OIDC against its trusted CA roots;
  # thumbprint is ignored but required by the API for some providers.
  thumbprint_list = ["6938fd4d98bab03faadb97b34396831e3780aea1"]
}

data "aws_iam_policy_document" "github_actions_assume_role" {
  statement {
    effect = "Allow"

    actions = ["sts:AssumeRoleWithWebIdentity"]

    principals {
      type        = "Federated"
      identifiers = [aws_iam_openid_connect_provider.github_actions.arn]
    }

    condition {
      test     = "StringEquals"
      variable = "token.actions.githubusercontent.com:aud"
      values   = ["sts.amazonaws.com"]
    }

    condition {
      test     = "StringLike"
      variable = "token.actions.githubusercontent.com:sub"
      values = [
        "repo:cabinetoffice/GCGS-Central-Digital-Platform:ref:refs/heads/main",
        "repo:cabinetoffice/GCGS-Central-Digital-Platform:ref:refs/heads/*-grafana-*",
      ]
    }
  }
}

resource "aws_iam_role" "github_actions_terraform" {
  name               = "${local.name_prefix}-terraform"
  assume_role_policy = data.aws_iam_policy_document.github_actions_assume_role.json
  tags               = var.tags
}

resource "aws_iam_policy" "github_actions_terraform" {
  name   = "${local.name_prefix}-terraform-github-oidc"
  policy = data.aws_iam_policy_document.github_actions_terraform.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "github_actions_terraform" {
  role       = aws_iam_role.github_actions_terraform.name
  policy_arn = aws_iam_policy.github_actions_terraform.arn
}
