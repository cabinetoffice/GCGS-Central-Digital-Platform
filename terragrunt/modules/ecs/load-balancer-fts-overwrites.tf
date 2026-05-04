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
