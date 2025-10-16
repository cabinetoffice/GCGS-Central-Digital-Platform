resource "aws_lb_target_group" "this" {
  count = var.host_port != null ? 1 : 0

  deregistration_delay = 30
  name_prefix          = local.tg_name_prefix
  port                 = var.host_port
  protocol             = "HTTP"
  target_type          = "ip"
  vpc_id               = var.vpc_id

  stickiness {
    type    = "lb_cookie"
    enabled = false
  }

  health_check {
    enabled             = true
    interval            = var.healthcheck_interval
    timeout             = var.healthcheck_timeout
    healthy_threshold   = var.healthcheck_healthy_threshold
    unhealthy_threshold = var.unhealthy_threshold
    path                = var.healthcheck_path
    port                = var.host_port
    protocol            = "HTTP"
    matcher             = "200"
  }

  lifecycle {
    create_before_destroy = true
  }

  tags = merge(
    {Service = var.name},
    var.tags
  )
}

resource "aws_lb_listener_rule" "this" {
  count = var.host_port != null ? 1 : 0

  listener_arn = var.ecs_listener_arn
  priority     = local.service_listener_rule_priority

  dynamic "action" {
    for_each = var.user_pool_arn != null && var.user_pool_client_id != null && var.user_pool_domain != null ? [1] : []

    content {
      type = "authenticate-cognito"
      authenticate_cognito {
        user_pool_arn              = var.user_pool_arn
        user_pool_client_id        = var.user_pool_client_id
        user_pool_domain           = var.user_pool_domain
        session_cookie_name        = "AWSELBAuthSessionCookie"
        scope                      = "openid"
        on_unauthenticated_request = "authenticate"
      }
      order = 1
    }
  }

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.this[0].arn
    order            = 2
  }

  condition {
    host_header {
      values = local.host_headers
    }
  }

  tags = merge(var.tags, { Name : var.name })
}

resource "aws_lb_listener_rule" "this_allowed_unauthenticated_paths" {
  count = var.host_port != null && length(var.allowed_unauthenticated_paths) > 0 ? 1 : 0

  listener_arn = var.ecs_listener_arn

  priority = local.service_listener_rule_priority - 55

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.this[0].arn
    order            = 1
  }

  condition {
    path_pattern {
      values = var.allowed_unauthenticated_paths
    }
  }

  condition {
    host_header {
      values = local.tg_host_header
    }
  }

  tags = merge(var.tags, { Name : var.name })
}
