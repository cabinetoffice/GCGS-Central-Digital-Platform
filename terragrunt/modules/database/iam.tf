resource "aws_iam_policy" "db_import" {
  name   = "${local.name_prefix}-db-import"
  policy = data.aws_iam_policy_document.db_import_handover_s3.json
}

resource "aws_iam_role_policy_attachment" "db_import_handover_s3" {
  role       = var.role_db_import_name
  policy_arn = aws_iam_policy.db_import.arn
}