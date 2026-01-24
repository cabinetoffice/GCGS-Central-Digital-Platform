resource "aws_iam_role" "opensearch_admin" {
  name = "${local.name_prefix}-opensearch-admin"

  assume_role_policy = data.aws_iam_policy_document.opensearch_admin_assume.json
}
