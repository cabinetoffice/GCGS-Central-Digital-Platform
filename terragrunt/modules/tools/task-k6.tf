# Seems not being used
# To be deprecated
#
# module "ecs_k6_tasks" {
#
#   source = "../ecs-service"
#
#   container_definitions = templatefile(
#     "${path.module}/templates/task-definitions/${var.tools_configs.k6.name}.json.tftpl",
#     {
#       auth_token    = "<To Be Pass At RunTime>"
#       cpu           = var.tools_configs.k6.cpu
#       duration      = "10s"
#       endpoints     = "getOrgs, postOrgs",
#       image         = "${local.orchestrator_account_id}.dkr.ecr.${data.aws_region.current.region}.amazonaws.com/cdp-${var.tools_configs.k6.name}:latest"
#       lg_name       = aws_cloudwatch_log_group.k6.name
#       lg_prefix     = "tools"
#       lg_region     = data.aws_region.current.region
#       max_vus       = 100
#       memory        = var.tools_configs.k6.memory
#       name          = var.tools_configs.k6.name
#       target_domain = var.public_domain
#       rps           = 15
#       vus           = 2
#     }
#   )
#
#   cluster_id             = var.ecs_cluster_id
#   container_port         = var.tools_configs.k6.port
#   cpu                    = var.tools_configs.k6.cpu
#   ecs_alb_sg_id          = var.alb_tools_sg_id
#   ecs_listener_arn       = aws_lb_listener.tools.arn
#   ecs_service_base_sg_id = var.ecs_sg_id
#   family                 = "tools"
#   is_standalone_task     = true
#   memory                 = var.tools_configs.k6.memory
#   name                   = var.tools_configs.k6.name
#   private_subnet_ids     = var.private_subnet_ids
#   product                = var.product
#   public_domain          = var.public_domain
#   role_ecs_task_arn      = var.role_ecs_task_arn
#   role_ecs_task_exec_arn = var.role_ecs_task_exec_arn
#   tags                   = var.tags
#   vpc_id                 = var.vpc_id
# }
