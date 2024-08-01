resource "aws_lb_target_group" "this" {
  count = var.host_port != null ? 1 : 0

  deregistration_delay = 30
  name                 = "cdp-${var.name}"
  port                 = var.host_port
  protocol             = "HTTP"
  target_type          = "ip"
  vpc_id               = var.vpc_id

  health_check {
    enabled           = true
    interval          = var.healthcheck_interval
    timeout           = var.healthcheck_timeout
    healthy_threshold = var.healthcheck_healthy_threshold
    path              = var.healthcheck_path
    port              = var.host_port
    protocol          = "HTTP"
    matcher           = "200"
  }

  lifecycle {
    create_before_destroy = false
  }
}


resource "aws_lb_listener_rule" "this" {
  count = var.host_port != null ? 1 : 0

  listener_arn = var.ecs_listener_arn
  priority     = var.family == "app" ? var.host_port - 8000 : var.host_port

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.this[0].arn
  }

  condition {
    host_header {
      values = var.is_frontend_app ? local.tg_host_header_with_alias : local.tg_host_header
    }
  }

  tags = merge(var.tags, { Name : var.name })
}
