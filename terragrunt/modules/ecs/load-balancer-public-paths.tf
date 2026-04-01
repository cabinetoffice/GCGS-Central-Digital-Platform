resource "aws_lb_listener_rule" "public_domain_fts_path_routing" {
  count = var.environment != "orchestrator" ? 1 : 0

  listener_arn = aws_lb_listener.ecs_php.arn
  priority     = 205

  action {
    type             = "forward"
    target_group_arn = module.ecs_service_fts_app.service_extra_target_group_arns["ov1"]
    order            = 1
  }

  condition {
    host_header {
      values = var.fts_extra_domains
    }
  }

  condition {
    path_pattern {
      values = ["/search/opportunities*", "/search/contracts*", "/contracts/*"]
    }
  }

  tags = merge(var.tags, { Name : "public-fts-path-routing" })
}

resource "aws_lb_listener_rule" "public_domain_fts_assets_path_routing" {
  count = var.environment != "orchestrator" ? 1 : 0

  listener_arn = aws_lb_listener.ecs_php.arn
  priority     = 206

  action {
    type             = "forward"
    target_group_arn = module.ecs_service_fts_app.service_extra_target_group_arns["ov1"]
    order            = 1
  }

  condition {
    host_header {
      values = var.fts_extra_domains
    }
  }

  condition {
    path_pattern {
      values = ["/css/*", "/js/*", "/images/*"]
    }
  }

  tags = merge(var.tags, { Name : "public-fts-assets-path-routing" })
}

resource "aws_lb_listener_rule" "public_domain_fts_assets_extra_path_routing" {
  count = var.environment != "orchestrator" ? 1 : 0

  listener_arn = aws_lb_listener.ecs_php.arn
  priority     = 207

  action {
    type             = "forward"
    target_group_arn = module.ecs_service_fts_app.service_extra_target_group_arns["ov1"]
    order            = 1
  }

  condition {
    host_header {
      values = var.fts_extra_domains
    }
  }

  condition {
    path_pattern {
      values = ["/assets/*", "/favicon.ico"]
    }
  }

  tags = merge(var.tags, { Name : "public-fts-assets-extra-path-routing" })
}

resource "aws_lb_listener_rule" "public_domain_fts_cpv_path_routing" {
  count = var.environment != "orchestrator" ? 1 : 0

  listener_arn = aws_lb_listener.ecs_php.arn
  priority     = 208

  action {
    type             = "forward"
    target_group_arn = module.ecs_service_fts_app.service_extra_target_group_arns["ov1"]
    order            = 1
  }

  condition {
    host_header {
      values = var.fts_extra_domains
    }
  }

  condition {
    path_pattern {
      values = ["/api/cpv-codes/search*"]
    }
  }

  tags = merge(var.tags, { Name : "public-fts-cpv-path-routing" })
}

resource "aws_lb_listener_rule" "public_domain_commercial_tools_path_routing" {
  count = var.environment != "orchestrator" ? 1 : 0

  listener_arn = aws_lb_listener.ecs_php.arn
  priority     = 106

  action {
    type             = "forward"
    target_group_arn = module.ecs_service_fts_app.service_extra_target_group_arns["ov1"]
    order            = 1
  }

  condition {
    host_header {
      values = var.fts_extra_domains
    }
  }

  condition {
    path_pattern {
      values = ["/search/commercial-tools*", "/commercial-tools*"]
    }
  }

  tags = merge(var.tags, { Name : "public-commercial-tools-path-routing" })
}
