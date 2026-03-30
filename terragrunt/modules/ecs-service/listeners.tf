resource "aws_lb_target_group" "external" {
  count = var.alb_enabled ? 1 : 0

  deregistration_delay = 30
  name                 = local.tg_name
  port                 = var.service_port
  protocol             = "HTTP"
  target_type          = "ip"
  vpc_id               = var.vpc_id

  stickiness {
    type    = "lb_cookie"
    enabled = false
  }

  health_check {
    enabled             = true
    healthy_threshold   = var.healthcheck_healthy_threshold
    interval            = var.healthcheck_interval
    matcher             = var.healthcheck_matcher
    path                = var.healthcheck_path
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = var.healthcheck_timeout
    unhealthy_threshold = var.unhealthy_threshold
  }

  lifecycle {
    create_before_destroy = true
  }

  tags = merge(
    { Service = var.name },
    var.tags
  )
}

resource "aws_lb_target_group" "external_extra" {
  for_each = var.alb_enabled ? { for rule in var.additional_external_target_groups : rule.name_suffix => rule } : {}

  deregistration_delay = 30
  name                 = "${local.tg_base_name_sanitized}-${each.key}"
  port                 = var.service_port
  protocol             = "HTTP"
  target_type          = "ip"
  vpc_id               = var.vpc_id

  stickiness {
    type    = "lb_cookie"
    enabled = false
  }

  health_check {
    enabled             = true
    healthy_threshold   = var.healthcheck_healthy_threshold
    interval            = var.healthcheck_interval
    matcher             = var.healthcheck_matcher
    path                = var.healthcheck_path
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = var.healthcheck_timeout
    unhealthy_threshold = var.unhealthy_threshold
  }

  lifecycle {
    create_before_destroy = true
  }

  tags = merge(
    { Service = var.name },
    var.tags
  )
}



moved {
  from = aws_lb_target_group.this
  to   = aws_lb_target_group.external
}

resource "aws_lb_target_group" "internal" {
  count = var.internal_alb_enabled ? 1 : 0

  deregistration_delay = 30
  name                 = local.internal_tg_name
  port                 = var.service_port
  protocol             = "HTTP"
  target_type          = "ip"
  vpc_id               = var.vpc_id

  stickiness {
    type    = "lb_cookie"
    enabled = false
  }

  health_check {
    enabled             = true
    healthy_threshold   = var.healthcheck_healthy_threshold
    interval            = var.healthcheck_interval
    matcher             = var.healthcheck_matcher
    path                = var.healthcheck_path
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = var.healthcheck_timeout
    unhealthy_threshold = var.unhealthy_threshold
  }

  lifecycle {
    create_before_destroy = true
  }

  tags = merge(
    { Service = var.name },
    var.tags
  )
}

moved {
  from = aws_lb_listener_rule.this
  to   = aws_lb_listener_rule.external
}

resource "aws_lb_listener_rule" "external" {
  count = var.alb_enabled ? 1 : 0

  listener_arn = var.ecs_listener_arn
  priority     = local.service_listener_rule_priority

  dynamic "action" {
    for_each = var.user_pool_arn != null && var.user_pool_client_id != null && var.user_pool_domain != null ? [1] : []

    content {
      type  = "authenticate-cognito"
      order = 1

      authenticate_cognito {
        user_pool_arn              = var.user_pool_arn
        user_pool_client_id        = var.user_pool_client_id
        user_pool_domain           = var.user_pool_domain
        session_cookie_name        = "AWSELBAuthSessionCookie"
        scope                      = "openid"
        on_unauthenticated_request = "authenticate"
      }
    }
  }

  action {
    type  = "forward"
    order = var.user_pool_arn != null && var.user_pool_client_id != null && var.user_pool_domain != null ? 2 : 1

    forward {
      target_group {
        arn = aws_lb_target_group.external[0].arn
      }
    }
  }

  condition {
    host_header {
      values = local.host_headers
    }
  }

  tags = merge(var.tags, { Name : var.name })
}

resource "aws_lb_listener_rule" "external_path_routing" {
  for_each = var.alb_enabled ? { for rule in var.path_routing_rules : tostring(rule.priority) => rule } : {}

  listener_arn = var.ecs_listener_arn
  priority     = each.value.priority

  action {
    type  = "forward"
    order = 1

    forward {
      target_group {
        arn = aws_lb_target_group.external[0].arn
      }
    }
  }

  condition {
    path_pattern {
      values = each.value.path_patterns
    }
  }

  condition {
    host_header {
      values = each.value.host_headers
    }
  }

  tags = merge(var.tags, { Name : "${var.name}-path-${each.key}" })
}

resource "aws_lb_listener_rule" "internal" {
  count = var.internal_alb_enabled && var.internal_listener_arn != null && var.internal_domain != null ? 1 : 0

  listener_arn = var.internal_listener_arn
  priority     = local.service_listener_rule_priority

  action {
    type  = "forward"
    order = 1

    forward {
      target_group {
        arn = aws_lb_target_group.internal[0].arn
      }
    }
  }

  condition {
    host_header {
      values = local.internal_host_headers
    }
  }

  tags = merge(var.tags, { Name : "${var.name}-internal" })
}

resource "aws_lb_listener_rule" "this_allowed_unauthenticated_paths" {
  count = var.alb_enabled && length(var.allowed_unauthenticated_paths) > 0 ? 1 : 0

  listener_arn = var.ecs_listener_arn

  priority = local.service_listener_rule_priority - 55

  action {
    type  = "forward"
    order = 1

    forward {
      target_group {
        arn = aws_lb_target_group.external[0].arn
      }
    }
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
