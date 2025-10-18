resource "aws_lb" "ecs_php" {
  drop_invalid_header_fields       = true
  enable_cross_zone_load_balancing = true
  enable_deletion_protection       = true
  idle_timeout                     = 60 * 5 // @TODO(ABN) Revisit, temp increase for DP-1910, DP-1929
  internal                         = false
  load_balancer_type               = "application"
  name                             = local.name_prefix_php
  security_groups                  = [var.alb_sg_id]
  subnets                          = var.public_subnet_ids

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ecs-php"
    }
  )
}

resource "aws_wafv2_web_acl_association" "ecs_php" {
  count = local.waf_enabled ? 1 : 0

  resource_arn = aws_lb.ecs_php.arn
  web_acl_arn  = var.waf_acl_php_arn
}

resource "aws_lb_listener" "ecs_php" {

  certificate_arn   = aws_acm_certificate.this.arn
  load_balancer_arn = aws_lb.ecs_php.arn
  port              = 443
  protocol          = "HTTPS"
  tags              = var.tags
  ssl_policy        = "ELBSecurityPolicy-TLS13-1-2-2021-06"

  default_action {
    type = "fixed-response"

    fixed_response {
      content_type = "text/plain"
      message_body = var.is_production ? "..." : "Fixed response from ${var.environment} environment's special services"
      status_code  = "200"
    }
  }
}

resource "aws_lb_listener" "ecs_php_http" {

  load_balancer_arn = aws_lb.ecs_php.arn
  port              = 80
  protocol          = "HTTP"
  tags              = var.tags

  default_action {
    redirect {
      status_code = "HTTP_301"
      protocol    = "HTTPS"
      port        = "443"
    }
    type = "redirect"
  }

}
