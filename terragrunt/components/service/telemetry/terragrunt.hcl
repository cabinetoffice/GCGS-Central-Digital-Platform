terraform {
  source = "../../../modules//telemetry"
}

include {
  path = find_in_parent_folders()
}

locals {
  global_vars  = read_terragrunt_config(find_in_parent_folders("terragrunt.hcl"))
  service_vars = read_terragrunt_config(find_in_parent_folders("service.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.service_vars.inputs.tags,
    {
      component = "telemetry"
    }
  )

}

dependency core_iam {
  config_path = "../../core/iam"
  mock_outputs = {
    ecs_task_arn      = "mock"
    ecs_task_name     = "mock"
    ecs_task_exec_arn = "mock"
    telemetry         = "mock"
  }
}

dependency core_networking {
  config_path = "../../core/networking"
  mock_outputs = {
    private_subnet_ids      = "mock"
    public_hosted_zone_fqdn = "mock"
    public_hosted_zone_id   = "mock"
    vpc_id                  = "mock"
  }
}

dependency core_security_groups {
  config_path = "../../core/security-groups"
  mock_outputs = {
    alb_sg_id = "mock"
    ecs_sg_id = "mock"
  }
}

dependency service_ecs {
  config_path = "../../service/ecs"
  mock_outputs = {
    ecs_cluster_id  = "mock"
    ecs_cluster_id  = "mock"
    ecs_lb_dns_name = "mock"
  }
}

inputs = {
  grafana_config  = local.global_vars.locals.tools_configs.grafana
  service_configs = local.global_vars.locals.service_configs
  tags            = local.tags

  role_ecs_task_arn      = dependency.core_iam.outputs.ecs_task_arn
  role_ecs_task_name      = dependency.core_iam.outputs.ecs_task_name
  role_ecs_task_exec_arn = dependency.core_iam.outputs.ecs_task_exec_arn
  role_telemetry_arn     = dependency.core_iam.outputs.telemetry

  private_subnet_ids      = dependency.core_networking.outputs.private_subnet_ids
  public_hosted_zone_fqdn = dependency.core_networking.outputs.public_hosted_zone_fqdn
  public_hosted_zone_id   = dependency.core_networking.outputs.public_hosted_zone_id
  vpc_id                  = dependency.core_networking.outputs.vpc_id

  ecs_alb_sg_id = dependency.core_security_groups.outputs.alb_sg_id
  ecs_sg_id     = dependency.core_security_groups.outputs.ecs_sg_id

  ecs_cluster_id   = dependency.service_ecs.outputs.ecs_cluster_id
  ecs_lb_dns_name  = dependency.service_ecs.outputs.ecs_lb_dns_name
  ecs_listener_arn = dependency.service_ecs.outputs.ecs_listener_arn
}
