module "ecs_service_data_sharing" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.data_sharing.name}.json.tftpl",
    {
      aspcore_environment = local.aspcore_environment
      container_port      = var.service_configs.data_sharing.port
      cpu                 = var.service_configs.data_sharing.cpu
      db_address          = var.db_sirsi_cluster_address
      db_name             = var.db_sirsi_cluster_name
      db_password         = local.db_sirsi_password
      db_username         = local.db_sirsi_username
      host_port           = var.service_configs.data_sharing.port
      image               = local.ecr_urls[var.service_configs.data_sharing.name]
      lg_name             = aws_cloudwatch_log_group.tasks[var.service_configs.data_sharing.name].name
      lg_prefix           = "app"
      lg_region           = data.aws_region.current.name
      memory              = var.service_configs.data_sharing.memory
      name                = var.service_configs.data_sharing.name
      public_domain       = var.public_domain
      s3_permanent_bucket = module.s3_bucket_permanent.bucket
      s3_staging_bucket   = module.s3_bucket_staging.bucket
      service_version     = local.service_version_sirsi
      vpc_cidr            = var.vpc_cider
    }
  )

  cluster_id             = local.main_cluster_id
  container_port         = var.service_configs.data_sharing.port
  cpu                    = var.service_configs.data_sharing.cpu
  desired_count          = var.service_configs.data_sharing.desired_count
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = local.main_ecs_listener_arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  host_port              = var.service_configs.data_sharing.port_host
  memory                 = var.service_configs.data_sharing.memory
  name                   = var.service_configs.data_sharing.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
