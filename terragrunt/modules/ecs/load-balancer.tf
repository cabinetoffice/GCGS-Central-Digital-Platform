resource "aws_lb" "ecs" {
  drop_invalid_header_fields       = true
  enable_cross_zone_load_balancing = true
  enable_deletion_protection       = true
  idle_timeout                     = var.environment != "staging" ?  60 :  60 * 5 // @TODO(ABN) Remove, temp increase for DP-1910, DP-1929
  internal                         = false
  load_balancer_type               = "application"
  name                             = local.name_prefix
  security_groups                  = [var.alb_sg_id]
  subnets                          = var.public_subnet_ids

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ecs"
    }
  )
}

resource "aws_wafv2_web_acl_association" "ecs" {
  count = local.waf_enabled ? 1 : 0

  resource_arn = aws_lb.ecs.arn
  web_acl_arn  = var.waf_acl_arn
}

resource "aws_lb_listener" "ecs" {

  certificate_arn   = aws_acm_certificate.this.arn
  load_balancer_arn = aws_lb.ecs.arn
  port              = 443
  protocol          = "HTTPS"
  tags              = var.tags
  ssl_policy        = "ELBSecurityPolicy-TLS13-1-2-2021-06"

  default_action {
    type = "fixed-response"

    fixed_response {
      content_type = "text/plain"
      message_body = var.is_production ? "..." : "Fixed response from ${var.environment} environment"
      status_code  = "200"
    }
  }
}

resource "aws_lb_listener" "ecs_http" {

  load_balancer_arn = aws_lb.ecs.arn
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

// To ensure overriding the authenticated rule. @TODO (ABN) (GO Live) Remove when removing Cognito
resource "aws_lb_listener_rule" "unauthenticated_assets" {

  listener_arn = local.main_ecs_listener_arn
  priority = 5

  action {
    type             = "forward"
    target_group_arn = module.ecs_service_organisation_app.service_target_group_arn
    order            = 1
  }

  condition {
    path_pattern {
      values = local.unauthenticated_assets_paths
    }
  }

  condition {
    host_header {
      values = [var.public_domain]
    }
  }

  tags = merge(
    var.tags,
    {
      Name : "Unauthenticated assets to ${var.service_configs.organisation_app.name}"
    }
  )
}
