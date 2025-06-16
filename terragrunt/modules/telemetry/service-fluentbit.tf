module "ecs_service_fluentbit" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.fluentbit_config.name}.json.tftpl",
    {
      container_path        = var.efs_fluentbit_container_path
      container_port        = var.fluentbit_config.port
      cpu                   = var.fluentbit_config.cpu
      host_port             = var.fluentbit_config.port
      image                 = "${local.orchestrator_account_id}.dkr.ecr.${data.aws_region.current.name}.amazonaws.com/cdp-${var.fluentbit_config.name}:latest"
      lg_name               = aws_cloudwatch_log_group.fluentbit.name
      lg_prefix             = "telemetry"
      lg_region             = data.aws_region.current.name
      memory                = var.fluentbit_config.memory
      name                  = var.fluentbit_config.name
      role_telemetry_arn    = var.role_telemetry_arn
      source_volume         = var.efs_fluentbit_volume_name
    }
  )

  cluster_id             = var.ecs_cluster_id
  container_port         = var.fluentbit_config.port
  cpu                    = var.fluentbit_config.cpu
  ecs_alb_sg_id          = var.ecs_alb_sg_id
  ecs_listener_arn       = var.ecs_listener_arn
  ecs_service_base_sg_id = var.ecs_sg_id
  efs_volume = {
    access_point_id    = var.efs_fluentbit_access_point_id
    container_path     = var.efs_fluentbit_container_path
    file_system_id     = var.efs_fluentbit_id
    iam                = "DISABLED"
    name               = var.efs_fluentbit_volume_name
    transit_encryption = "ENABLED"
  }
  family                 = "telemetry"
  healthcheck_path       = "/"
  host_port              = var.fluentbit_config.port
  memory                 = var.fluentbit_config.memory
  name                   = var.fluentbit_config.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id = var.vpc_id
}
