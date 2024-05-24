resource "aws_sfn_state_machine" "ecs_force_deploy" {
  for_each = toset(local.services)

  name     = "${local.name_prefix}-deploy-${each.value}"
  role_arn = var.role_service_deployer_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/update-service.json.tftpl", {
    cluster = aws_ecs_cluster.this.name,
    service = each.value
  })
}

resource "aws_sfn_state_machine" "ecs_run_migration" {
  name     = "${local.name_prefix}-run-${local.organisation_information_migrations.name}"
  role_arn = var.role_service_deployer_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/run-task.json.tftpl", {
    cluster         = aws_ecs_cluster.this.name,
    security_groups = var.ecs_sg_id
    subnet          = var.private_subnet_ids[0]
    task            = local.organisation_information_migrations.name
    task_definition = module.ecs_service_organisation_information_migrations.task_definition_arn
  })
}
