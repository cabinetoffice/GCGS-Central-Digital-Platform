resource "aws_sfn_state_machine" "ecs_force_deploy" {
  for_each = toset(local.services)

  name     = "${local.name_prefix}-deploy-${each.value}"
  role_arn = var.role_service_deployer_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/deployment.json.tftpl", {
    cluster = aws_ecs_cluster.this.name,
    service = each.value
  })
}
