data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_secretsmanager_secret" "pen_testing_configuration" {
  name = "${local.name_prefix}-pen-testing-configuration"
}

data "aws_secretsmanager_secret_version" "pen_testing_configuration" {
  secret_id = data.aws_secretsmanager_secret.pen_testing_configuration.id
}

data "aws_secretsmanager_secret" "terraform_operators" {
  name = "${local.name_prefix}-terraform-operators"
}

data "aws_secretsmanager_secret_version" "terraform_operators" {
  secret_id = data.aws_secretsmanager_secret.terraform_operators.id
}

data "aws_iam_policy_document" "github_actions_terraform" {
  statement {
    sid     = "ReadGrafanaSecrets"
    effect  = "Allow"
    actions = [
      "secretsmanager:DescribeSecret",
      "secretsmanager:GetSecretValue",
    ]
    resources = [
      "arn:aws:secretsmanager:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:secret:${local.name_prefix}-grafana-api-token*",
      "arn:aws:secretsmanager:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:secret:${local.name_prefix}-grafana-alerting*",
    ]
  }

  statement {
    sid     = "KmsDecryptSecrets"
    effect  = "Allow"
    actions = ["kms:Decrypt"]
    resources = ["*"]
    condition {
      test     = "StringEquals"
      variable = "kms:ViaService"
      values   = ["secretsmanager.${data.aws_region.current.region}.amazonaws.com"]
    }
  }
}
