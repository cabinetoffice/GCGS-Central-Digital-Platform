module "ecs_service_fluentbit" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.fluentbit_config.name}.json.tftpl",
    {
      container_port        = var.fluentbit_config.port
      cpu                   = var.fluentbit_config.cpu
      image                 = "amazon/aws-for-fluent-bit:latest"
      lg_name               = aws_cloudwatch_log_group.fluentbit.name
      lg_prefix             = "telemetry"
      lg_region             = data.aws_region.current.name
      memory                = var.fluentbit_config.memory
      name                  = var.fluentbit_config.name
      role_telemetry_arn    = var.role_telemetry_arn
    }
  )

  cluster_id             = var.ecs_cluster_id
  container_port         = var.fluentbit_config.port
  cpu                    = var.fluentbit_config.cpu
  ecs_alb_sg_id          = var.ecs_alb_sg_id
  ecs_listener_arn       = var.ecs_listener_arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "telemetry"
  is_standalone_task     = true
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
