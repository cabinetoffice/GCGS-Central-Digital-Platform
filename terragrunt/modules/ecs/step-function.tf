resource "aws_sfn_state_machine" "ecs_force_deploy" {
  for_each = local.service_configs

  name     = "${local.name_prefix}-deploy-${each.value.name}"
  role_arn = var.role_service_deployer_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/update-service.json.tftpl", {
    cluster = each.value.cluster == "sirsi-php" ? local.php_cluster_name : each.value.cluster == "fts" ? local.fts_cluster_name : local.main_cluster_name,
    service = each.value.name
  })
}

# Runnable ECS tasks (db-migrations + one-off tasks) invoked via Step Functions.
resource "aws_sfn_state_machine" "ecs_run_task" {
  for_each = local.runnable_task_runners

  name     = "${local.name_prefix}-run-${each.key}"
  role_arn = var.role_service_deployer_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/run-task.json.tftpl", {
    cluster         = each.value.cluster
    security_groups = var.ecs_sg_id
    subnet          = var.private_subnet_ids[0]
    task            = each.key
    task_definition = each.value.task_definition
  })
}
