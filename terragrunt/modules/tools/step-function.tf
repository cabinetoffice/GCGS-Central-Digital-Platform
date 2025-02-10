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
