resource "aws_iam_user" "github_user" {
  name = "${local.name_prefix}-github-user"
  tags = var.tags
}

resource "aws_iam_user_policy" "ecr_push_policy" {
  name   = "ECRPushPolicy"
  user   = aws_iam_user.github_user.name
  policy = data.aws_iam_policy_document.ecr_push_policy.json
}

resource "aws_iam_user_policy" "ssm_update_policy" {
  name   = "SSMUpdatePolicy"
  user   = aws_iam_user.github_user.name
  policy = data.aws_iam_policy_document.ssm_update_policy.json
}

resource "aws_iam_access_key" "github_user_access_key" {
  user = aws_iam_user.github_user.name
}

resource "aws_iam_role" "orchestrator_read_service_version" {
  assume_role_policy = data.aws_iam_policy_document.orchestrator_read_service_version_assume_role.json
  name               = "${local.name_prefix}-${var.environment}-read-service-version"
  tags               = var.tags
}

resource "aws_iam_policy" "orchestrator_read_service_version" {
  description = "Policy to allow reading SSM parameters in the orchestrator account"
  name        = "${local.name_prefix}-${var.environment}-read-service-version"
  policy      = data.aws_iam_policy_document.orchestrator_read_service_version.json
}

resource "aws_iam_role_policy_attachment" "orchestrator_read_service_version" {
  policy_arn = aws_iam_policy.orchestrator_read_service_version.arn
  role       = aws_iam_role.orchestrator_read_service_version.name
}

resource "aws_iam_role_policy_attachment" "orchestrator_terraform_read_service_version" {
  policy_arn = aws_iam_policy.orchestrator_read_service_version.arn
  role       = data.aws_iam_role.terraform.name
}
