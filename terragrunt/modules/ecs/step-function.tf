resource "aws_sfn_state_machine" "ecs_force_deploy" {
  for_each = local.service_configs

  name     = "${local.name_prefix}-deploy-${each.value.name}"
  role_arn = var.role_service_deployer_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/update-service.json.tftpl", {
    cluster = contains(["fts", "cfs"], each.value.name) && local.is_php_migrated_env ? aws_ecs_cluster.that.name : aws_ecs_cluster.this.name,
    service = each.value.name
  })
}

resource "aws_sfn_state_machine" "ecs_run_migration" {
  for_each = local.migration_configs_sirsi
  name     = "${local.name_prefix}-run-${each.value.name}"
  role_arn = var.role_service_deployer_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/run-task.json.tftpl", {
    cluster         = aws_ecs_cluster.this.name,
    security_groups = var.ecs_sg_id
    subnet          = var.private_subnet_ids[0]
    task            = each.value.name
    task_definition = module.ecs_migration_tasks[each.key].task_definition_arn
  })
}

resource "aws_sfn_state_machine" "ecs_run_migration_cfs" {
  for_each = local.migration_configs_cfs
  name     = "${local.name_prefix}-run-${each.value.name}"
  role_arn = var.role_service_deployer_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/run-task.json.tftpl", {
    cluster         = local.php_cluster_name
    security_groups = var.ecs_sg_id
    subnet          = var.private_subnet_ids[0]
    task            = each.value.name
    task_definition = module.ecs_migration_tasks_cfs[each.key].task_definition_arn
  })
}

resource "aws_sfn_state_machine" "ecs_run_migration_fts" {
  for_each = local.migration_configs_fts
  name     = "${local.name_prefix}-run-${each.value.name}"
  role_arn = var.role_service_deployer_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/run-task.json.tftpl", {
    cluster         = local.php_cluster_name
    security_groups = var.ecs_sg_id
    subnet          = var.private_subnet_ids[0]
    task            = each.value.name
    task_definition = module.ecs_migration_tasks_fts[each.key].task_definition_arn
  })
}
