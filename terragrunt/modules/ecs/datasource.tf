data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_secretsmanager_secret" "one_login" {
  name = "${local.name_prefix}-one-login-credentials"
}

data "aws_iam_policy_document" "ecs_task_exec" {
  statement {
    sid    = "AllowAccessToProductSecrets"
    effect = "Allow"

    actions = [
      "secretsmanager:GetSecretValue"
    ]
    resources = [
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:cdp-sirsi-*"
    ]
  }
  statement {
    sid    = "AllowDecryptSecrets"
    effect = "Allow"

    actions = [
      "kms:Decrypt"
    ]
    resources = [
      var.db_kms_arn
    ]
  }
}

data "aws_iam_policy_document" "cloudwatch_event_invoke_deployer_step_function" {
  statement {
    actions   = ["states:StartExecution"]
    resources = [for sm in aws_sfn_state_machine.ecs_force_deploy : sm.arn]
  }
}

data "aws_iam_policy_document" "service_deployer_step_function" {
  statement {
    actions   = ["ecs:UpdateService"]
    resources = [
      for service in local.services :
      "arn:aws:ecs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:service/${aws_ecs_cluster.this.name}/${service}"
    ]
  }
}
