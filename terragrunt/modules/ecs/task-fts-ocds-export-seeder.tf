module "ecs_task_fts_ocds_export_seeder" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.ocds_export_seeder.name}.json.tftpl",
    merge(
      local.fts_dotnet_common,
      {
        aws_region            = data.aws_region.current.region
        cpu                   = var.service_configs.ocds_export_seeder.cpu
        db_address            = local.fts_db_address
        db_name               = var.db_fts_cluster_name
        db_password           = local.db_fts_password
        db_port               = 3306
        db_username           = local.db_fts_username
        image                 = local.ecr_urls[var.service_configs.ocds_export_seeder.name]
        lg_name               = aws_cloudwatch_log_group.tasks[var.service_configs.ocds_export_seeder.name].name
        lg_prefix             = "app"
        lg_region             = data.aws_region.current.region
        memory                = var.service_configs.ocds_export_seeder.memory
        name                  = var.service_configs.ocds_export_seeder.name
        ocds_export_queue_url = var.queue_ocds_export_url
      }
    )
  )

  alb_enabled            = false
  cluster_id             = local.fts_cluster_id
  cpu                    = var.service_configs.ocds_export_seeder.cpu
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "standalone"
  is_standalone_task     = true
  memory                 = var.service_configs.ocds_export_seeder.memory
  name                   = var.service_configs.ocds_export_seeder.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}

