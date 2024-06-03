resource "aws_lb" "ecs" {
  drop_invalid_header_fields       = true
  enable_cross_zone_load_balancing = true
  enable_deletion_protection       = true
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

resource "aws_lb_listener" "ecs_http" {

  load_balancer_arn = aws_lb.ecs.arn
  port              = 80
  protocol          = "HTTP"
  tags              = var.tags

  default_action {
    type = "fixed-response"

    fixed_response {
      content_type = "text/plain"
      message_body = "Fixed response from ${var.environment} environment"
      status_code  = "200"
    }
  }

}
