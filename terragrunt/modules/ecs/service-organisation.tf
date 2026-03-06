module "ecs_service_organisation" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.organisation.name}.json.tftpl",
    {
      internal_service_urls           = local.internal_service_urls
      public_service_urls             = local.public_service_urls
      use_internal_service_urls       = local.use_internal_service_urls
      use_internal_issuer             = local.use_internal_issuer
      aspcore_environment             = local.aspcore_environment
      cpu                             = var.service_configs.organisation.cpu
      db_address                      = var.db_sirsi_cluster_address
      db_name                         = var.db_sirsi_cluster_name
      db_password                     = local.db_sirsi_password
      db_username                     = local.db_sirsi_username
      govuknotify_apikey              = data.aws_secretsmanager_secret_version.govuknotify_apikey.arn
      govuknotify_support_admin_email = data.aws_secretsmanager_secret_version.govuknotify_support_admin_email.arn
      image                           = local.ecr_urls[var.service_configs.organisation.name]
      internal_service_urls           = local.internal_service_urls
      lg_name                         = aws_cloudwatch_log_group.tasks[var.service_configs.organisation.name].name
      lg_prefix                       = "app"
      lg_region                       = data.aws_region.current.region
      memory                          = var.service_configs.organisation.memory
      name                            = var.service_configs.organisation.name
      public_domain                   = var.public_domain
      queue_entity_verification_url   = var.queue_entity_verification_url
      queue_organisation_url          = var.queue_organisation_url
      send_notify_emails              = local.send_notify_emails
      service_port                    = local.service_ports_by_service[var.service_configs.organisation.name]
      service_version                 = local.service_version_sirsi
      vpc_cidr                        = var.vpc_cider
    }
  )

  cluster_id             = local.main_cluster_id
  cpu                    = var.service_configs.organisation.cpu
  desired_count          = var.service_configs.organisation.desired_count
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = local.main_ecs_listener_arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  internal_alb_enabled   = local.use_internal_service_urls
  internal_domain        = local.internal_domain
  internal_listener_arn  = local.internal_ecs_listener_arn
  listener_priority      = var.service_configs.organisation.listener_priority
  memory                 = var.service_configs.organisation.memory
  name                   = var.service_configs.organisation.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  public_domain          = var.public_domain
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  service_port           = local.service_ports_by_service[var.service_configs.organisation.name]
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
