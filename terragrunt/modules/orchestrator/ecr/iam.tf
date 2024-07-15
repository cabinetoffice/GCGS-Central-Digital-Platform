resource "aws_ecr_repository_policy" "policy" {
  for_each = toset(local.repositories)

  policy     = data.aws_iam_policy_document.ecr_repo_policy_document.json
  repository = aws_ecr_repository.this[each.key].name
}


