resource "aws_codepipeline" "trigger_update_account" {
  name = "${local.name_prefix}-${local.trigger_update_account_cp_name}"

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
    name = "Update-Orchestrator-Account"
    action {
      category         = "Build"
      input_artifacts  = ["source_output"]
      name             = "${local.name_prefix}-update-account-in-orchestrator"
      output_artifacts = ["output_update_account_orchestrator"]
      owner            = "AWS"
      provider         = "CodeBuild"
      version          = "1"
      configuration = {
        ProjectName = "${local.name_prefix}-update-account-in-orchestrator"
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
    name = "Update-Development-Account"
    action {
      category         = "Build"
      input_artifacts  = ["source_output"]
      name             = "${local.name_prefix}-update-account-in-development"
      output_artifacts = ["output_update_account_development"]
      owner            = "AWS"
      provider         = "CodeBuild"
      version          = "1"
      configuration = {
        ProjectName = "${local.name_prefix}-update-account-in-development"
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
    name = "Approve-Updating-Staging-Account"
    action {
      name     = "ManualApproval"
      category = "Approval"
      owner    = "AWS"
      provider = "Manual"
      version  = "1"
    }
  }

  stage {
    name = "Update-Staging-Account"
    action {
      category         = "Build"
      input_artifacts  = ["source_output"]
      name             = "${local.name_prefix}-update-account-in-staging"
      output_artifacts = ["output_update_account_staging"]
      owner            = "AWS"
      provider         = "CodeBuild"
      version          = "1"
      configuration = {
        ProjectName = "${local.name_prefix}-update-account-in-staging"
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
    name = "Approve-Updating-Integration-Account"
    action {
      name     = "ManualApproval"
      category = "Approval"
      owner    = "AWS"
      provider = "Manual"
      version  = "1"
    }
  }

  stage {
    name = "Update-Integration-Account"
    action {
      category         = "Build"
      input_artifacts  = ["source_output"]
      name             = "${local.name_prefix}-update-account-in-integration"
      output_artifacts = ["output_update_account_integration"]
      owner            = "AWS"
      provider         = "CodeBuild"
      version          = "1"
      configuration = {
        ProjectName = "${local.name_prefix}-update-account-in-integration"
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
