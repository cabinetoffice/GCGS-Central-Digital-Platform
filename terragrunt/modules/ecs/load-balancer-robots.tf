resource "aws_lb_listener_rule" "public_domain_robots_main" {
  count = !var.is_production ? 1 : 0

  listener_arn = aws_lb_listener.ecs.arn
  priority     = 4

  action {
    type = "fixed-response"

    fixed_response {
      content_type = "text/plain"
      message_body = "User-agent: *\nDisallow: /\n"
      status_code  = "200"
    }
  }

  condition {
    path_pattern {
      values = ["/robots.txt"]
    }
  }

  tags = merge(var.tags, { Name : "public-robots-txt-main" })
}

resource "aws_lb_listener_rule" "public_domain_robots_php" {
  count = !var.is_production ? 1 : 0

  listener_arn = aws_lb_listener.ecs_php.arn
  priority     = 104

  action {
    type = "fixed-response"

    fixed_response {
      content_type = "text/plain"
      message_body = "User-agent: *\nDisallow: /\n"
      status_code  = "200"
    }
  }

  condition {
    path_pattern {
      values = ["/robots.txt"]
    }
  }

  tags = merge(var.tags, { Name : "public-robots-txt-php" })
}

resource "aws_lb_listener_rule" "public_domain_robots_fts" {
  count = !var.is_production ? 1 : 0

  listener_arn = aws_lb_listener.ecs_fts.arn
  priority     = 104

  action {
    type = "fixed-response"

    fixed_response {
      content_type = "text/plain"
      message_body = "User-agent: *\nDisallow: /\n"
      status_code  = "200"
    }
  }

  condition {
    path_pattern {
      values = ["/robots.txt"]
    }
  }

  tags = merge(var.tags, { Name : "public-robots-txt-fts" })
}
