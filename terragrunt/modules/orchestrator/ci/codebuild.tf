resource "aws_codebuild_project" "this" {
  for_each      = local.deployment_environments

  build_timeout = 5
  description   = "Deploy infra and application changes into ${each.key}"
  name          = "${local.name_prefix}-deployment-to-${each.key}"
  service_role  = var.ci_build_role_arn
  tags          = var.tags

  artifacts {
    type = "CODEPIPELINE"
  }

  cache {
    type = "NO_CACHE"
  }

  environment {
    compute_type                = "BUILD_GENERAL1_LARGE"
    image_pull_credentials_type = "SERVICE_ROLE"
    image                       = "${var.repository_urls["codebuild"]}:latest"
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
      group_name  = aws_cloudwatch_log_group.deployments[each.key].name
      stream_name = local.name_prefix
    }
  }

  source {
    type      = "CODEPIPELINE"
    buildspec = file("${path.module}/buildspecs/deployment.yml")
  }

  vpc_config {
    vpc_id             = var.vpc_id
    security_group_ids = [var.ci_sg_id]
    subnets            = var.private_subnet_ids
  }
}
