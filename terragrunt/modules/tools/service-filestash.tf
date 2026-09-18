locals {
  filestash_storage_label = "e2e-reports"

  filestash_s3_mapping = {
    (local.filestash_storage_label) = {
      type   = "s3"
      region = data.aws_region.current.region
      path   = "/${local.filestash_reports_bucket_name}/"
    }
  }

  filestash_config_json = jsonencode({
    general = {
      host = "${var.filestash_config.name}.${var.public_domain}"
    }
    connections = [
      {
        type  = "s3"
        label = local.filestash_storage_label
      }
    ]
    middleware = {
      identity_provider = {
        type   = "passthrough"
        params = jsonencode({ strategy = "direct" })
      }
      attribute_mapping = {
        related_backend = local.filestash_storage_label
        params          = jsonencode(local.filestash_s3_mapping)
      }
    }
  })

  filestash_config_b64 = base64encode(local.filestash_config_json)
}

module "ecs_service_filestash" {
  source = "../ecs-service"

  container_definitions = templatefile(
    "${path.module}/templates/task-definitions/filestash.json.tftpl",
    {
      admin_password       = "${aws_secretsmanager_secret.filestash_credentials.arn}:ADMIN_PASSWORD::"
      cpu                  = var.filestash_config.cpu
      filestash_config_b64 = local.filestash_config_b64
      image                = var.filestash_image
      lg_name              = aws_cloudwatch_log_group.filestash.name
      lg_prefix            = "tools"
      lg_region            = data.aws_region.current.region
      memory               = var.filestash_config.memory
      name                 = var.filestash_config.name
      public_hostname      = "${var.filestash_config.name}.${var.public_domain}"
      service_port         = var.filestash_config.port
    }
  )

  auth_session_cookie_name = "AWSELBAuthSessionCookieE2EReports"
  cluster_id               = var.ecs_cluster_id
  cpu                      = var.filestash_config.cpu
  desired_count            = var.environment == "development" ? 1 : 0
  ecs_alb_sg_id            = var.alb_tools_sg_id
  ecs_listener_arn         = aws_lb_listener.tools.arn
  ecs_service_base_sg_id   = var.ecs_sg_id
  family                   = "tools"
  fixed_response_rules = [
    {
      priority      = 44999
      path_patterns = ["/admin*", "/admin/*"]
      status_code   = "403"
      content_type  = "text/plain"
      message_body  = "Forbidden"
    }
  ]
  healthcheck_path        = "/about"
  listener_priority       = 45000
  memory                  = var.filestash_config.memory
  name                    = var.filestash_config.name
  private_subnet_ids      = var.private_subnet_ids
  product                 = var.product
  public_domain           = var.public_domain
  role_ecs_task_arn       = var.role_filestash_task_arn
  role_ecs_task_exec_arn  = var.role_ecs_task_exec_arn
  service_port            = var.filestash_config.port
  tags                    = var.tags
  user_pool_arn           = var.user_pool_arn_tools
  user_pool_client_id     = var.user_pool_client_id_tools_filestash
  user_pool_domain        = var.user_pool_domain_tools
  vpc_id                  = var.vpc_id
}

