resource "aws_sfn_state_machine" "ecs_force_deploy" {
  name     = "${local.name_prefix}-api-gateway-deployer"
  role_arn = var.role_api_gateway_deployer_step_function_arn
  tags     = var.tags

  definition = templatefile("${path.module}/templates/state-machine/api-gateway-deployment.json.tftpl", {
    rest_api_id = aws_api_gateway_rest_api.ecs_api.id,
    stage_name  = aws_api_gateway_stage.ecs_api.stage_name
  })
}
