module "ecs_service_ocds_exporter" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.ocds_exporter.name}.json.tftpl",
    merge(
      local.fts_dotnet_common,
      {
        aws_region            = data.aws_region.current.region
        cpu                   = var.service_configs.ocds_exporter.cpu
        db_address            = local.fts_db_address
        db_name               = var.db_fts_cluster_name
        db_password           = local.db_fts_password
        db_port               = 3306
        db_username           = local.db_fts_username
        image                 = local.ecr_urls[var.service_configs.ocds_exporter.name]
        lg_name               = aws_cloudwatch_log_group.tasks[var.service_configs.ocds_exporter.name].name
        lg_prefix             = "app"
        lg_region             = data.aws_region.current.region
        memory                = var.service_configs.ocds_exporter.memory
        name                  = var.service_configs.ocds_exporter.name
        ocds_export_queue_url = var.queue_ocds_export_url
        ocds_exports_bucket   = module.s3_bucket_ocds_exports.bucket
      }
    )
  )

  alb_enabled                        = false
  cluster_id                         = local.fts_cluster_id
  cpu                                = var.service_configs.ocds_exporter.cpu
  deployment_maximum_percent         = 100
  deployment_minimum_healthy_percent = 0
  # Run only in development for now; other envs stay disabled until the one-off task is ready.
  desired_count                      = var.environment == "development" ? 1 : 0
  ecs_service_base_sg_id             = var.ecs_sg_id
  family                             = "standalone"
  memory                             = var.service_configs.ocds_exporter.memory
  name                               = var.service_configs.ocds_exporter.name
  private_subnet_ids                 = var.private_subnet_ids
  product                            = var.product
  role_ecs_task_arn                  = var.role_ecs_task_arn
  role_ecs_task_exec_arn             = var.role_ecs_task_exec_arn
  tags                               = var.tags
  vpc_id                             = var.vpc_id
}
