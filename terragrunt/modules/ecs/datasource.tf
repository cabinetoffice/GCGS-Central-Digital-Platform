data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_secretsmanager_secret" "authority_keys" {
  name = "${local.name_prefix}-authority-keys"
}

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
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:rds!*",
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
    resources = concat([for sm in aws_sfn_state_machine.ecs_force_deploy : sm.arn], [aws_sfn_state_machine.ecs_run_migration.arn])
  }
}

data "aws_iam_policy_document" "step_function_manage_services" {
  statement {
    actions = ["ecs:UpdateService"]
    resources = [
      for service in local.services :
      "arn:aws:ecs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:service/${aws_ecs_cluster.this.name}/${service}"
    ]
    sid = "MangeECSService"
  }

  statement {
    actions = ["ecs:RunTask"]
    resources = [
      for task in local.tasks :
      "arn:aws:ecs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:task-definition/*${task}:*"
    ]
    sid = "MangeECSTask"
  }

  statement {
    actions = [
      "events:PutRule",
      "events:PutTargets",
    ]
    resources = [
      "arn:aws:events:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:rule/StepFunctionsGetEventsForECSTaskRule"
    ]
    sid = "MangeEvents"
  }

  statement {
    actions = [
      "iam:PassRole",
    ]
    resources = [
      var.role_ecs_task_arn,
      var.role_ecs_task_exec_arn
    ]
    sid = "MangeIAM"
  }
}

data "aws_iam_policy_document" "ecr_pull_from_orchestrator" {
  statement {
    actions = [
      "ecr:GetDownloadUrlForLayer",
      "ecr:BatchGetImage",
      "ecr:GetAuthorizationToken"
    ]
    resources = ["*"]
  }
}

# Configure the provider to assume the role in the orchestrator account and fetch the latest service version
provider "aws" {
  alias  = "orchestrator"
  region = "eu-west-2"
}

provider "aws" {
  alias  = "orchestrator_assume_role"
  region = "eu-west-2"
  assume_role {
    role_arn = "arn:aws:iam::${local.orchestrator_account_id}:role/cdp-sirsi-orchestrator-read-service-version"
  }
}

data "aws_ssm_parameter" "orchestrator_service_version" {
  provider = aws.orchestrator_assume_role
  name     = "/cdp-sirsi-service-version"
}
