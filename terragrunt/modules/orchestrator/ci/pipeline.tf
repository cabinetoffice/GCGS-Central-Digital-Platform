resource "aws_codepipeline" "this" {

  execution_mode = "SUPERSEDED"
  name           = "${local.name_prefix}-deployment"
  pipeline_type  = "V1"
  role_arn       = var.ci_pipeline_role_arn

  artifact_store {
    type     = "S3"
    location = module.s3_bucket.bucket
  }

  stage {
    name = "Pull"
    action {
      name             = "Source"
      category         = "Source"
      owner            = "AWS"
      provider         = "CodeStarSourceConnection"
      version          = "1"
      output_artifacts = ["source_output"]
      configuration = {
        ConnectionArn    = data.aws_codestarconnections_connection.cabinet_office.arn
        DetectChanges    = false
        FullRepositoryId = "cabinetoffice/GCGS-Central-Digital-Platform"
        BranchName       = "BAU-slack-notifications"
      }
    }
  }

  stage {
    name = "Update-Orchestrator"
    action {
      category         = "Build"
      input_artifacts  = ["source_output"]
      name             = "${local.name_prefix}-deployment-to-orchestrator"
      output_artifacts = ["output_update_account_orchestrator"]
      owner            = "AWS"
      provider         = "CodeBuild"
      version          = "1"
      configuration = {
        ProjectName = "${local.name_prefix}-deployment-to-orchestrator"
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
    name = "Update-Development"
    action {
      category         = "Build"
      input_artifacts  = ["source_output"]
      name             = "${local.name_prefix}-deployment-to-development"
      output_artifacts = ["output_update_account_development"]
      owner            = "AWS"
      provider         = "CodeBuild"
      version          = "1"
      configuration = {
        ProjectName = "${local.name_prefix}-deployment-to-development"
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
    name = "Wait-for-pproval-to-update-Staging"
    action {
      name     = "ManualApproval"
      category = "Approval"
      owner    = "AWS"
      provider = "Manual"
      version  = "1"
    }
  }

  stage {
    name = "Update-Staging"
    action {
      category         = "Build"
      input_artifacts  = ["source_output"]
      name             = "${local.name_prefix}-deployment-to-staging"
      output_artifacts = ["output_update_account_staging"]
      owner            = "AWS"
      provider         = "CodeBuild"
      version          = "1"
      configuration = {
        ProjectName = "${local.name_prefix}-deployment-to-staging"
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
    name = "Wait-for-approval-to-update-Integration"
    action {
      name     = "ManualApproval"
      category = "Approval"
      owner    = "AWS"
      provider = "Manual"
      version  = "1"
    }
  }

  stage {
    name = "Update-Integration"
    action {
      category         = "Build"
      input_artifacts  = ["source_output"]
      name             = "${local.name_prefix}-deployment-to-integration"
      output_artifacts = ["output_update_account_integration"]
      owner            = "AWS"
      provider         = "CodeBuild"
      version          = "1"
      configuration = {
        ProjectName = "${local.name_prefix}-deployment-to-integration"
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
