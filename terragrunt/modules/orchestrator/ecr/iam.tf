resource "aws_ecr_repository_policy" "policy" {
  for_each = toset(local.repositories)

  policy     = data.aws_iam_policy_document.ecr_repo_policy_document.json
  repository = aws_ecr_repository.this[each.key].name
}

resource "aws_iam_role" "orchestrator_read_service_version" {
  assume_role_policy = data.aws_iam_policy_document.orchestrator_read_service_version_assume_role.json
  name               = "${local.name_prefix}-orchestrator-read-service-version"
  tags               = var.tags
}

resource "aws_iam_policy" "orchestrator_read_service_version" {
  description = "Policy to allow reading SSM parameters in the orchestrator account"
  name        = "${local.name_prefix}-orchestrator-read-service-version"
  policy      = data.aws_iam_policy_document.orchestrator_read_service_version.json
}

resource "aws_iam_role_policy_attachment" "orchestrator_read_service_version" {
  policy_arn = aws_iam_policy.orchestrator_read_service_version.arn
  role       = aws_iam_role.orchestrator_read_service_version.name
}
