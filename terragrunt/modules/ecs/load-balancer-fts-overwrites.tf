resource "aws_lb_listener_rule" "authenticate_pcr2015_to_new_fts" {
  listener_arn = aws_lb_listener.ecs_php.arn
  priority     = 100

  action {
    type  = "authenticate-cognito"
    order = 1

    authenticate_cognito {
      user_pool_arn              = var.user_pool_pcr2015_arn
      user_pool_client_id        = var.user_pool_pcr2015_client_id
      user_pool_domain           = var.user_pool_pcr2015_domain
      session_cookie_name        = "AWSELBAuthSessionCookie"
      scope                      = "openid"
      on_unauthenticated_request = "authenticate"
    }
  }

  action {
    type             = "forward"
    target_group_arn = module.ecs_service_fts_app.service_extra_target_group_arns["ov1"]
    order            = 2
  }

  condition {
    host_header {
      values = var.fts_extra_domains
    }
  }

  condition {
    path_pattern {
      values = ["/pcr-2015/*", "/*/pcr-2015/*"]
    }
  }

  tags = merge(var.tags, { Name : "authenticate-pcr2015-to-new-fts" })
}

resource "aws_lb_listener_rule" "overwrite_to_new_fts" {
  for_each = {
    for idx, paths in local.overwrite_to_new_fts_path_groups :
    idx => paths
  }

  listener_arn = aws_lb_listener.ecs_php.arn
  priority     = 101 + each.key

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
      values = each.value
    }
  }

  tags = merge(var.tags, { Name : "overwrite-to-new-fts-${each.key}" })
}
