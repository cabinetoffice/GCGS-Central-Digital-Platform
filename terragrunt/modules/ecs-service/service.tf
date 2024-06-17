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
}

resource "aws_ecs_service" "this" {
  count = contains(["app", "telemetry"], var.family) ? 1 : 0

  name            = var.name
  cluster         = var.cluster_id
  task_definition = aws_ecs_task_definition.this.arn
  desired_count   = var.desired_count
  launch_type     = "FARGATE"

  network_configuration {
    assign_public_ip = false
    security_groups  = [var.ecs_service_base_sg_id]
    subnets          = var.private_subnet_ids
  }

  dynamic "load_balancer" {
    for_each = aws_lb_target_group.this
    content {
      target_group_arn = load_balancer.value.arn
      container_name   = var.name
      container_port   = var.container_port
    }
  }

  tags = var.tags
}
