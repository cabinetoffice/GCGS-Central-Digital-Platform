data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_iam_policy_document" "cloudwatch_event_invoke_tools_deployer_step_function" {
  statement {
    actions = ["states:StartExecution"]
    resources = concat(
      [for fd in aws_sfn_state_machine.ecs_tools_force_deploy : fd.arn],
    )
  }
}

data "aws_iam_policy_document" "step_function_manage_tools_services" {
  statement {
    actions = ["ecs:UpdateService"]
    resources = [
      for service, config in local.auto_redeploy_tools_service_configs :
      "arn:aws:ecs:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:service/${var.ecs_cluster_name}/${service}"
    ]
    sid = "MangeECSService"
  }

  statement {
    actions = ["ecs:RunTask"]
    resources = [
      for task in local.executable_tasks_by_step_functions :
      "arn:aws:ecs:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:task-definition/*${task}:*"
    ]
    sid = "MangeECSTask"
  }

  statement {
    actions = [
      "events:PutRule",
      "events:PutTargets",
    ]
    resources = [
      "arn:aws:events:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:rule/StepFunctionsGetEventsForECSTaskRule"
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
