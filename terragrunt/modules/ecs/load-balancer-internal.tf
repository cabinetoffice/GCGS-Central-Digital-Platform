resource "aws_lb" "ecs_internal" {
  drop_invalid_header_fields       = true
  enable_cross_zone_load_balancing = true
  enable_deletion_protection       = true
  idle_timeout                     = 60 * 5
  internal                         = true
  load_balancer_type               = "application"
  name                             = "${local.name_prefix}-internal"
  security_groups                  = [var.alb_internal_sg_id]
  subnets                          = var.private_subnet_ids

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ecs-internal"
    }
  )

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_lb_listener" "ecs_internal" {
  certificate_arn   = aws_acm_certificate.internal.arn
  load_balancer_arn = aws_lb.ecs_internal.arn
  port              = 443
  protocol          = "HTTPS"
  tags              = var.tags
  ssl_policy        = "ELBSecurityPolicy-TLS13-1-2-2021-06"

  default_action {
    type = "fixed-response"

    fixed_response {
      content_type = "text/plain"
      message_body = var.is_production ? "..." : "Fixed response from ${var.environment} environment (internal)"
      status_code  = "200"
    }
  }
}
