data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_iam_policy_document" "ecs_task_decrypt_pgadmin_secrets" {
  statement {
    sid    = "AllowDecryptPGAdminSecrets"
    effect = "Allow"

    actions = [
      "kms:Decrypt"
    ]
    resources = [module.rds_pgadmin.db_kms_arn]
  }
}
