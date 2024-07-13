resource "aws_ecr_repository_policy" "policy" {
  for_each = toset(local.repositories)

  repository = aws_ecr_repository.this[each.key].name

  policy = data.aws_iam_policy_document.ecr_repo_policy_document.json
}
