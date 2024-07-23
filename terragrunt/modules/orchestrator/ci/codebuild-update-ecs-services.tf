resource "aws_codebuild_project" "update_ecs_services" {
  for_each      = { for key, value in var.account_ids : key => value if !contains(["production", "orchestrator"], key) }
  name          = "${local.name_prefix}-${local.update_ecs_service_cb_name}-in-${each.key}"
  description   = "Run terraform in service/ecs component to only update ECS services"
  service_role  = var.ci_build_role_arn
  build_timeout = 5

  artifacts {
    type = "CODEPIPELINE"
  }

  environment {
    compute_type                = "BUILD_GENERAL1_SMALL"
    image_pull_credentials_type = "SERVICE_ROLE"
    image                       = "${data.aws_caller_identity.current.account_id}.dkr.ecr.${data.aws_region.current.name}.amazonaws.com/cdp-codebuild:latest"
    type                        = "LINUX_CONTAINER"
    privileged_mode             = true
    environment_variable {
      name  = "AWS_REGION"
      value = data.aws_region.current.name
    }
    environment_variable {
      name  = "AWS_ACCOUNT_ID"
      value = each.value
    }
    environment_variable {
      name  = "TG_ENVIRONMENT"
      value = each.key
    }
  }

  logs_config {
    cloudwatch_logs {
      group_name  = aws_cloudwatch_log_group.update_ecs_services.name
      stream_name = local.name_prefix
    }
  }

  source {
    type      = "CODEPIPELINE"
    buildspec = file("${path.module}/buildspecs/${local.update_ecs_service_cb_name}.yml")
  }

  vpc_config {
    vpc_id             = var.vpc_id
    security_group_ids = [var.ci_sg_id]
    subnets            = var.private_subnet_ids
  }

  cache {
    type = "NO_CACHE"
  }

  tags = var.tags
}
