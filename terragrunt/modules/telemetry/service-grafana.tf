module "ecs_service_grafana" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/grafana.json.tftpl",
    {
      account_id         = data.aws_caller_identity.current.account_id
      container_port     = var.grafana_config.port
      cpu                = var.grafana_config.cpu
      gf_admin_password  = "${aws_secretsmanager_secret.grafana_credentials.arn}:ADMIN_PASSWORD::"
      gf_admin_user      = "${aws_secretsmanager_secret.grafana_credentials.arn}:ADMIN_USERNAME::"
      host_port          = var.grafana_config.port
      image              = "${local.orchestrator_account_id}.dkr.ecr.${data.aws_region.current.name}.amazonaws.com/cdp-grafana:latest"
      lg_name            = aws_cloudwatch_log_group.grafana.name
      lg_prefix          = "telemetry"
      lg_region          = data.aws_region.current.name
      memory             = var.grafana_config.memory
      name               = var.grafana_config.name
      role_telemetry_arn = var.role_telemetry_arn
    }
  )

  cluster_id             = var.ecs_cluster_id
  container_port         = var.grafana_config.port
  cpu                    = var.grafana_config.cpu
  ecs_alb_sg_id          = var.ecs_alb_sg_id
  ecs_listener_arn       = var.ecs_listener_arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "telemetry"
  healthcheck_path       = "/api/health"
  host_port              = var.grafana_config.port
  memory                 = var.grafana_config.memory
  name                   = var.grafana_config.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
