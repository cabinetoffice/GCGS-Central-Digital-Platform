resource "aws_lb" "tools" {
  drop_invalid_header_fields       = true
  enable_cross_zone_load_balancing = true
  enable_deletion_protection       = true
  idle_timeout                     = 60 * 5
  internal                         = false
  load_balancer_type               = "application"
  name                             = "${local.name_prefix}-tools"
  security_groups                  = [var.alb_tools_sg_id]
  subnets                          = var.public_subnet_ids

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-tools"
    }
  )
}

resource "aws_wafv2_web_acl_association" "tools" {
  resource_arn = aws_lb.tools.arn
  web_acl_arn  = var.waf_acl_tools_arn
}

resource "aws_lb_listener" "tools" {

  certificate_arn   = var.certificate_arn
  load_balancer_arn = aws_lb.tools.arn
  port              = 443
  protocol          = "HTTPS"
  tags              = var.tags
  ssl_policy        = "ELBSecurityPolicy-TLS13-1-2-2021-06"

  default_action {
    type = "fixed-response"

    fixed_response {
      content_type = "text/plain"
      message_body = var.is_production ? "..." : "Fixed response from tools in ${var.environment} environment"
      status_code  = "200"
    }
  }
}

resource "aws_lb_listener" "tools_http" {

  load_balancer_arn = aws_lb.tools.arn
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
