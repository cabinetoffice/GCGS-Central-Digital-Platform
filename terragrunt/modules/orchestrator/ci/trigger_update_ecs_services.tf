resource "aws_codepipeline" "trigger_update_ecs_services" {
  name = "${local.name_prefix}-${local.trigger_update_ecs_service_cb_name}"

  role_arn = var.ci_pipeline_role_arn

  artifact_store {
    type     = "S3"
    location = module.s3_bucket.bucket
  }

  stage {
    name = "Source"
    action {
      name             = "Source"
      category         = "Source"
      owner            = "AWS"
      provider         = "ECR"
      version          = "1"
      output_artifacts = ["source_output"]
      configuration = {
        RepositoryName = "cdp-codebuild"
        ImageTag       = "latest"
      }
    }
  }

  stage {
    name = "Update-Ecs-In-Development"
    action {
      category         = "Build"
      input_artifacts  = ["source_output"]
      name             = "${local.name_prefix}-update-ecs-services-in-development"
      output_artifacts = ["output_update_ecs_development"]
      owner            = "AWS"
      provider         = "CodeBuild"
      version          = "1"
      configuration = {
        ProjectName = "${local.name_prefix}-update-ecs-services-in-development"
        EnvironmentVariables = jsonencode([
          {
            name  = "ENABLE_STATUS_CHECKS"
            type  = "PLAINTEXT"
            value = "False"
          }
        ])
      }
    }
  }

  stage {
    name = "Approve-Updating-ECS-in-Staging"
    action {
      name     = "ManualApproval"
      category = "Approval"
      owner    = "AWS"
      provider = "Manual"
      version  = "1"
    }
  }

  stage {
    name = "Update-Ecs-In-Staging"
    action {
      category         = "Build"
      input_artifacts  = ["source_output"]
      name             = "${local.name_prefix}-update-ecs-services-in-staging"
      output_artifacts = ["output_update_ecs_staging"]
      owner            = "AWS"
      provider         = "CodeBuild"
      version          = "1"
      configuration = {
        ProjectName = "${local.name_prefix}-update-ecs-services-in-staging"
        EnvironmentVariables = jsonencode([
          {
            name  = "ENABLE_STATUS_CHECKS"
            type  = "PLAINTEXT"
            value = "False"
          }
        ])
      }
    }
  }

  stage {
    name = "Approve-Updating-ECS-in-Integration"
    action {
      name     = "ManualApproval"
      category = "Approval"
      owner    = "AWS"
      provider = "Manual"
      version  = "1"
    }
  }

  stage {
    name = "Update-Ecs-In-Integration"
    action {
      category         = "Build"
      input_artifacts  = ["source_output"]
      name             = "${local.name_prefix}-update-ecs-services-in-integration"
      output_artifacts = ["output_update_ecs_integration"]
      owner            = "AWS"
      provider         = "CodeBuild"
      version          = "1"
      configuration = {
        ProjectName = "${local.name_prefix}-update-ecs-services-in-integration"
        EnvironmentVariables = jsonencode([
          {
            name  = "ENABLE_STATUS_CHECKS"
            type  = "PLAINTEXT"
            value = "False"
          }
        ])
      }
    }
  }

  tags = var.tags
}
