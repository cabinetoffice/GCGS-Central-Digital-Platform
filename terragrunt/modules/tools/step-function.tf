resource "aws_sfn_state_machine" "ecs_tools_force_deploy" {
  for_each = local.auto_redeploy_tools_service_configs

  name     = "${local.name_prefix}-tools-deploy-${each.value.name}"
  role_arn = var.role_service_deployer_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/update-service.json.tftpl", {
    cluster = var.ecs_cluster_name,
    service = each.value.name
  })
}

resource "aws_sfn_state_machine" "ecs_run_performance_test" {
  name     = "${local.name_prefix}-run-${var.tools_configs.k6.name}"
  role_arn = var.role_service_deployer_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/run-task.json.tftpl", {
    cluster         = var.ecs_cluster_name
    security_groups = var.ecs_sg_id
    subnet          = var.private_subnet_ids[0]
    task            = var.tools_configs.k6.name
    task_definition = module.ecs_k6_tasks.task_definition_arn
  })
}
