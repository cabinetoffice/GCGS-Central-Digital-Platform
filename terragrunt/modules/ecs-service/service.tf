resource "aws_ecs_task_definition" "this" {
  container_definitions    = var.container_definitions
  cpu                      = var.cpu
  execution_role_arn       = var.role_ecs_task_exec_arn
  family                   = "${var.family}-${var.name}"
  memory                   = var.memory
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  tags                     = var.tags
  task_role_arn            = var.role_ecs_task_arn

  dynamic "volume" {
    for_each = var.efs_volume == null ? [] : [var.efs_volume]
    content {
      name = volume.value.name

      efs_volume_configuration {
        file_system_id     = volume.value.file_system_id
        transit_encryption = volume.value.transit_encryption

        authorization_config {
          access_point_id = volume.value.access_point_id
          iam             = volume.value.iam
        }
      }
    }
  }
}

resource "aws_ecs_service" "this" {
  count = var.is_standalone_task ? 0 : 1

  availability_zone_rebalancing      = var.deployment_maximum_percent <= 100 ? "DISABLED" : "ENABLED"
  name                               = var.name
  cluster                            = var.cluster_id
  task_definition                    = aws_ecs_task_definition.this.arn
  desired_count                      = var.desired_count
  launch_type                        = "FARGATE"
  deployment_maximum_percent         = var.deployment_maximum_percent
  deployment_minimum_healthy_percent = var.deployment_minimum_healthy_percent
  force_new_deployment               = var.force_new_deployment
  wait_for_steady_state              = false
  health_check_grace_period_seconds  = var.alb_enabled || var.internal_alb_enabled ? var.health_check_grace_period_seconds : null

  depends_on = [
    time_sleep.listener_rule_propagation
  ]

  network_configuration {
    assign_public_ip = false
    security_groups  = [var.ecs_service_base_sg_id]
    subnets          = var.private_subnet_ids
  }

  dynamic "load_balancer" {
    for_each = var.alb_enabled ? aws_lb_target_group.external : []
    content {
      target_group_arn = load_balancer.value.arn
      container_name   = var.name
      container_port   = var.service_port
    }
  }

  dynamic "load_balancer" {
    for_each = var.alb_enabled ? aws_lb_target_group.external_extra : {}
    content {
      target_group_arn = load_balancer.value.arn
      container_name   = var.name
      container_port   = var.service_port
    }
  }

  dynamic "load_balancer" {
    for_each = var.internal_alb_enabled ? aws_lb_target_group.internal : []
    content {
      target_group_arn = load_balancer.value.arn
      container_name   = var.name
      container_port   = var.service_port
    }
  }

  tags = var.tags
}

resource "time_sleep" "listener_rule_propagation" {
  depends_on = [
    aws_lb_listener_rule.external,
    aws_lb_listener_rule.internal,
    aws_lb_listener_rule.this_allowed_unauthenticated_paths
  ]

  create_duration = var.listener_rule_propagation_delay
}
