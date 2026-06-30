resource "aws_lb" "ecs_fts" {
  drop_invalid_header_fields       = true
  enable_cross_zone_load_balancing = true
  enable_deletion_protection       = true
  idle_timeout                     = 60 * 5
  internal                         = false
  load_balancer_type               = "application"
  name                             = local.name_prefix_fts
  security_groups                  = [var.alb_sg_id]
  subnets                          = var.public_subnet_ids

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ecs-fts"
    }
  )
}

resource "aws_wafv2_web_acl_association" "ecs_fts" {
  count = local.waf_enabled ? 1 : 0

  resource_arn = aws_lb.ecs_fts.arn
  web_acl_arn  = var.waf_acl_fts_arn
}

resource "aws_lb_listener" "ecs_fts" {

  certificate_arn   = aws_acm_certificate.this.arn
  load_balancer_arn = aws_lb.ecs_fts.arn
  port              = 443
  protocol          = "HTTPS"
  tags              = var.tags
  ssl_policy        = "ELBSecurityPolicy-TLS13-1-2-2021-06"

  default_action {
    type = "fixed-response"

    fixed_response {
      content_type = "text/plain"
      message_body = var.is_production ? "..." : "Fixed response from ${var.environment} environment's FTS services"
      status_code  = "200"
    }
  }
}

resource "aws_lb_listener" "ecs_fts_http" {

  load_balancer_arn = aws_lb.ecs_fts.arn
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

resource "aws_lb_listener_rule" "authenticate_pcr2015_fts_app" {
  listener_arn = aws_lb_listener.ecs_fts.arn
  priority     = 200

  action {
    type  = "authenticate-cognito"
    order = 1

    authenticate_cognito {
      user_pool_arn              = var.user_pool_fts_arn
      user_pool_client_id        = var.user_pool_fts_client_id
      user_pool_domain           = var.user_pool_fts_domain
      session_cookie_name        = "AWSELBAuthSessionCookie"
      scope                      = "openid"
      on_unauthenticated_request = "authenticate"
    }
  }

  action {
    type             = "forward"
    target_group_arn = module.ecs_service_fts_app.service_target_group_arn
    order            = 2
  }

  condition {
    host_header {
      values = distinct(concat(["${var.service_configs.fts_app.name}.${var.public_domain}"], var.fts_extra_host_headers))
    }
  }

  condition {
    path_pattern {
      values = ["/pcr-2015/*", "/*/pcr-2015/*"]
    }
  }

  tags = merge(var.tags, { Name : "authenticate-pcr2015-fts-app" })
}
