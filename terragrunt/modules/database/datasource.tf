data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_iam_policy_document" "step_function_manage_secrets" {
  statement {
    actions = ["secretsmanager:GetSecretValue"]
    resources = [
      data.aws_secretsmanager_secret.postgres.id,
    ]
    sid = "FetchDBCredentials"
  }
  statement {
    actions = [
      "kms:Decrypt",
      "kms:Encrypt",
      "kms:GenerateDataKey"
    ]
    resources = [aws_kms_key.rds.arn]
    sid       = "AccessToDBKey"
  }
  statement {
    actions   = ["secretsmanager:PutSecretValue"]
    resources = [aws_secretsmanager_secret.db_connection_string.id]
    sid       = "UpdateDBConnectionString"
  }
}

data "aws_iam_policy_document" "cloudwatch_event_invoke_db_connection_string_step_function" {
  statement {
    actions   = ["states:StartExecution"]
    resources = [aws_sfn_state_machine.update_connection_string.arn]
  }
}
