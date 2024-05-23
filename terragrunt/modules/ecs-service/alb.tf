resource "aws_lb_listener" "ecs_http" {
  count = var.listening_port != null ? 1 : 0

  load_balancer_arn = var.ecs_alb_arn
  port              = var.listening_port
  protocol          = "HTTP"
  tags              = var.tags

  default_action {
    target_group_arn = aws_lb_target_group.this[0].arn
    type             = "forward"
  }
  #   default_action {
  #     redirect {
  #       status_code = "HTTP_301"
  #       protocol    = "HTTPS"
  #       port        = "443"
  #     }
  #     type = "redirect"
  #   }
}

resource "aws_lb_target_group" "this" {
  count = var.listening_port != null ? 1 : 0

  deregistration_delay = 30
  name                 = "cdp-${var.name}"
  port                 = var.host_port
  protocol             = "HTTP"
  target_type          = "ip"
  vpc_id               = var.vpc_id

  health_check {
    enabled           = true
    interval          = 120
    timeout           = 60
    healthy_threshold = 10
    path              = "/health"
    port              = var.host_port
    protocol          = "HTTP"
    matcher           = "200"
  }

  lifecycle {
    create_before_destroy = false
  }
}
