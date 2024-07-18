resource "aws_codebuild_project" "update_account" {
  name          = "${local.name_prefix}-${local.update_account_cb_name}"
  description   = "Run terraform in components updating entire account"
  service_role  = var.ci_build_role_arn
  build_timeout = 5

  artifacts {
    type = "CODEPIPELINE"
  }

  environment {
    compute_type                = "BUILD_GENERAL1_SMALL"
    image_pull_credentials_type = "SERVICE_ROLE"
    image                       = "${local.orchestrator_account_id}.dkr.ecr.${data.aws_region.current.name}.amazonaws.com/cdp-codebuild:latest"
    type                        = "LINUX_CONTAINER"
    privileged_mode             = true
    environment_variable {
      name  = "AWS_REGION"
      value = data.aws_region.current.name
    }
    environment_variable {
      name  = "AWS_ACCOUNT_ID"
      value = data.aws_caller_identity.current.account_id
    }
    environment_variable {
      name  = "TG_ENVIRONMENT"
      value = var.environment
    }
  }

  logs_config {
    cloudwatch_logs {
      group_name  = aws_cloudwatch_log_group.update_account.name
      stream_name = local.name_prefix
    }
  }

  source {
    type      = "CODEPIPELINE"
    buildspec = "./terragrunt/buildspecs/${local.update_account_cb_name}.yml"
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
