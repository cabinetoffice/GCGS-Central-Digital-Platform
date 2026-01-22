resource "aws_iam_role" "db_import" {
  name               = "${local.name_prefix}-db-import"
  assume_role_policy = data.aws_iam_policy_document.db_import_assume.json
}
