resource "random_password" "cdp_sirsi_diagnostic_path" {
  length           = 32
  special          = true
  override_special = "-"
}

resource "aws_secretsmanager_secret" "cdp_sirsi_diagnostic_path" {
  name        = "${local.name_prefix}-diagnostic-path"
  description = "Stores the frontend diagnostic path"

  lifecycle {
    prevent_destroy = true # Prevent the secret from being destroyed
  }

  tags = var.tags
}

resource "aws_secretsmanager_secret_version" "cdp_sirsi_diagnostic_path" {
  secret_id     = aws_secretsmanager_secret.cdp_sirsi_diagnostic_path.id
  secret_string = "/${random_password.cdp_sirsi_diagnostic_path.result}"
}

module "ecs_service_organisation_app" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/${var.service_configs.organisation_app.name}.json.tftpl",
    {
      aspcore_environment                 = local.aspcore_environment
      charity_commission_url              = "${data.aws_secretsmanager_secret.charity_commission.arn}:Url::"
      charity_commission_subscription_key = "${data.aws_secretsmanager_secret.charity_commission.arn}:SubscriptionKey::"
      companies_house_url                 = "${data.aws_secretsmanager_secret.companies_house.arn}:Url::"
      companies_house_user                = "${data.aws_secretsmanager_secret.companies_house.arn}:User::"
      container_port                      = var.service_configs.organisation_app.port
      cpu                                 = var.service_configs.organisation_app.cpu
      fts_service_url_arn                 = data.aws_secretsmanager_secret_version.fts_service_url.arn
      host_port                           = var.service_configs.organisation_app.port
      image                               = local.ecr_urls[var.service_configs.organisation_app.name]
      diagnostic_page_enabled             = !var.is_production || var.environment == "integration"
      diagnostic_page_path_arn            = aws_secretsmanager_secret.cdp_sirsi_diagnostic_path.arn
      lg_name                             = aws_cloudwatch_log_group.tasks[var.service_configs.organisation_app.name].name
      lg_prefix                           = "app"
      lg_region                           = data.aws_region.current.name
      memory                              = var.service_configs.organisation_app.memory
      name                                = var.service_configs.organisation_app.name
      onelogin_account_url                = local.one_loging.credential_locations.account_url
      onelogin_authority                  = local.one_loging.credential_locations.authority
      onelogin_client_id                  = local.one_loging.credential_locations.client_id
      onelogin_private_key                = local.one_loging.credential_locations.private_key
      public_domain                       = var.public_domain
      s3_permanent_bucket                 = module.s3_bucket_permanent.bucket
      s3_staging_bucket                   = module.s3_bucket_staging.bucket
      service_version                     = local.service_version
      vpc_cidr                            = var.vpc_cider
    }
  )

  cluster_id                    = aws_ecs_cluster.this.id
  container_port                = var.service_configs.organisation_app.port
  cpu                           = var.service_configs.organisation_app.cpu
  desired_count                 = var.service_configs.organisation_app.desired_count
  ecs_alb_sg_id                 = var.alb_sg_id
  ecs_listener_arn              = aws_lb_listener.ecs.arn
  ecs_service_base_sg_id        = var.ecs_sg_id
  family                        = "app"
  host_port                     = var.service_configs.organisation_app.port
  is_frontend_app               = true
  memory                        = var.service_configs.organisation_app.memory
  name                          = var.service_configs.organisation_app.name
  private_subnet_ids            = var.private_subnet_ids
  product                       = var.product
  public_domain                 = var.public_domain
  role_ecs_task_arn             = var.role_ecs_task_arn
  role_ecs_task_exec_arn        = var.role_ecs_task_exec_arn
  tags                          = var.tags
  allowed_unauthenticated_paths = ["/one-login/back-channel-sign-out", "/assets/*", "/css/*", "/manifest.json"]
  user_pool_arn                 = var.user_pool_arn
  user_pool_client_id           = var.user_pool_client_id
  user_pool_domain              = var.user_pool_domain
  vpc_id                        = var.vpc_id
}
