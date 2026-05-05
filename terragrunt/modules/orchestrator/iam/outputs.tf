output "fts_github_user_arn" {
  value = aws_iam_user.github_user.arn
}

output "github_user_access_key_id" {
  value = aws_iam_access_key.github_user_access_key.id
}

output "github_user_secret_access_key" {
  value     = aws_iam_access_key.github_user_access_key.secret
  sensitive = true
}
