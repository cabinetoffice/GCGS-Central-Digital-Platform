module "ecs_service_authority" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.authority.name}.json.tftpl",
    {
      aspcore_environment     = local.aspcore_environment
      authority_private_key   = "${data.aws_secretsmanager_secret.authority_keys.arn}:PRIVATE::"
      conn_string_location    = var.db_sirsi_connection_secret_arn
      container_port          = var.service_configs.authority.port
      cpu                     = var.service_configs.authority.cpu
      host_port               = var.service_configs.authority.port
      image                   = local.ecr_urls[var.service_configs.authority.name]
      lg_name                 = aws_cloudwatch_log_group.tasks[var.service_configs.authority.name].name
      lg_prefix               = "app"
      lg_region               = data.aws_region.current.name
      memory                  = var.service_configs.authority.memory
      name                    = var.service_configs.authority.name
      oi_db_address           = var.db_sirsi_address
      oi_db_name              = var.db_sirsi_name
      oi_db_password          = "${var.db_sirsi_credentials_arn}:username::"
      oi_db_username          = "${var.db_sirsi_credentials_arn}:password::"
      onelogin_authority      = local.one_loging.credential_locations.authority
      onelogin_client_id      = local.one_loging.credential_locations.client_id
      onelogin_private_key    = local.one_loging.credential_locations.private_key
      public_hosted_zone_fqdn = var.public_hosted_zone_fqdn
      service_version         = local.service_version
      vpc_cidr                = var.vpc_cider
    }
  )

  cluster_id             = aws_ecs_cluster.this.id
  container_port         = var.service_configs.authority.port
  cpu                    = var.service_configs.authority.cpu
  ecs_alb_sg_id          = var.alb_sg_id
  ecs_listener_arn       = aws_lb_listener.ecs.arn
  ecs_service_base_sg_id = var.ecs_sg_id
  family                 = "app"
  host_port              = var.service_configs.authority.port_host
  memory                 = var.service_configs.authority.memory
  name                   = var.service_configs.authority.name
  private_subnet_ids     = var.private_subnet_ids
  product                = var.product
  role_ecs_task_arn      = var.role_ecs_task_arn
  role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
  tags                   = var.tags
  vpc_id                 = var.vpc_id
}
