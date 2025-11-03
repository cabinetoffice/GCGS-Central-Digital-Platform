data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

# Configure the provider to assume the role in the orchestrator account and fetch the latest service version
provider "aws" {
  alias  = "orchestrator"
  region = "eu-west-2"
}
provider "aws" {
  alias  = "orchestrator_assume_role"
  region = "eu-west-2"
  assume_role {
    role_arn = "arn:aws:iam::${local.orchestrator_account_id}:role/${local.name_prefix}-orchestrator-read-service-version"
  }
}

data "aws_ssm_parameter" "orchestrator_cfs_service_version" {
  provider = aws.orchestrator_assume_role
  name     = "/${local.name_prefix}-cfs-service-version"
}

data "aws_ssm_parameter" "orchestrator_fts_service_version" {
  provider = aws.orchestrator_assume_role
  name     = "/${local.name_prefix}-fts-service-version"
}

data "aws_ssm_parameter" "orchestrator_sirsi_service_version" {
  provider = aws.orchestrator_assume_role
  name     = "/${local.name_prefix}-service-version"
}

data "aws_secretsmanager_secret" "authority_keys" {
  name = "${local.name_prefix}-authority-keys"
}

data "aws_secretsmanager_secret" "charity_commission" {
  name = "${local.name_prefix}-charity-commission-credentials"
}

data "aws_secretsmanager_secret" "odi_data_platform" {
  name = "${local.name_prefix}-odi-data-platform"
}

data "aws_secretsmanager_secret" "companies_house" {
  name = "${local.name_prefix}-companies-house-credentials"
}
data "aws_secretsmanager_secret" "redis_auth_token" {
  arn = var.redis_auth_token_arn
}

data "aws_secretsmanager_secret" "cfs_secrets" {
  name = "${local.name_prefix}-cfs/secrets"
}

data "aws_secretsmanager_secret" "fts_secrets" {
  name = "${local.name_prefix}-fts/secrets"
}

data "aws_secretsmanager_secret_version" "fts_service_url" {
  secret_id = "${local.name_prefix}-fts-service-url"
}

data "aws_secretsmanager_secret_version" "govuknotify_apikey" {
  secret_id = "${local.name_prefix}-govuknotify-apikey"
}

data "aws_secretsmanager_secret_version" "govuknotify_support_admin_email" {
  secret_id = "${local.name_prefix}-govuknotify-support-admin-email"
}

data "aws_secretsmanager_secret" "one_login_credentials" {
  name = "${local.name_prefix}-one-login-credentials"
}

data "aws_secretsmanager_secret" "one_login_forward_logout_notification_api_key" {
  name = "${local.name_prefix}-one-login-forward-logout-notification-api-key"
}

data "aws_iam_policy_document" "ecs_task_access_secrets" {
  statement {
    sid    = "AllowAccessToProductSecrets"
    effect = "Allow"

    actions = [
      "secretsmanager:GetSecretValue"
    ]
    resources = [
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:${local.name_prefix}-*",
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:rds!*",
      "arn:aws:secretsmanager:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:secret:rds-*"
    ]
  }
  statement {
    sid    = "AllowDecryptSecrets"
    effect = "Allow"

    actions = [
      "kms:Decrypt"
    ]
    resources = [
      var.db_ev_cluster_credentials_kms_key_id,
      var.db_sirsi_cluster_credentials_kms_key_id,
    ]
  }
}

data "aws_iam_policy_document" "ecs_task_access_queue" {
  statement {
    sid    = "AllowAccessToProductSQS"
    effect = "Allow"
    actions = [
      "sqs:DeleteMessage",
      "sqs:GetQueueAttributes",
      "sqs:GetQueueUrl",
      "sqs:ReceiveMessage",
      "sqs:SendMessage",
    ]
    resources = [
      var.queue_av_scanner_arn,
      var.queue_entity_verification_arn,
      var.queue_organisation_arn
    ]
  }
}

data "aws_iam_policy_document" "ecs_task_access_elasticache" {
  statement {
    sid    = "AllowAppToManageItsOwnDataProtectionToken"
    effect = "Allow"
    actions = [
      "ssm:GetParametersByPath",
      "ssm:PutParameter"
    ]
    resources = [
      "arn:aws:ssm:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:parameter/${local.name_prefix}-ec-*"
    ]
  }
}

data "aws_iam_policy_document" "ecs_task_access_ses" {
  statement {
    sid    = "AllowAppToSendEmails"
    effect = "Allow"
    actions = [
      "ses:SendEmail",
      "ses:SendRawEmail"
    ]
    resources = [
      "arn:aws:ses:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:identity/${local.ses_identity_domain}",
      "arn:aws:ses:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:identity/*.service.gov.uk",
      "arn:aws:ses:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:identity/*goaco.com",
      var.ses_configuration_set_arn
    ]
  }
}

data "aws_iam_policy_document" "ecs_task_serilog" {
  statement {
    sid    = "AllowSerilogCreateStreamAndLog"
    effect = "Allow"
    actions = [
      "logs:Create*",
      "logs:Describe*",
      "logs:Put*",

    ]
    resources = ["arn:aws:logs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:*"]
  }
}

data "aws_iam_policy_document" "cloudwatch_event_invoke_deployer_step_function" {
  statement {
    actions = ["states:StartExecution"]
    resources = concat(
      [for fd in aws_sfn_state_machine.ecs_force_deploy : fd.arn],
      [for rm in aws_sfn_state_machine.ecs_run_migration : rm.arn]
    )
  }
}

data "aws_iam_policy_document" "step_function_manage_services" {
  statement {
    actions = ["ecs:UpdateService"]

    resources = concat(
      [
        for service, config in local.service_configs :
        "arn:aws:ecs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:service/${local.main_cluster_name}/${service}"
      ],
      [
        for service, config in local.service_configs :
        "arn:aws:ecs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:service/${local.php_cluster_name}/${service}"
      ]
    )

    sid = "ManageECSService"
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

