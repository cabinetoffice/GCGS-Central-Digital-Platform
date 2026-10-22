resource "aws_codepipeline" "weekly_refresh" {
  name           = "${local.name_prefix}-fts-db-weekly-refresh"
  pipeline_type  = "V1"
  execution_mode = "SUPERSEDED"
  role_arn       = var.ci_pipeline_role_arn

  artifact_store {
    type     = "S3"
    location = module.artifacts_bucket.bucket
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
        BranchName       = var.source_branch_name
      }
    }
  }

  stage {
    name = "Prod-Snapshot-Share"

    action {
      category         = "Build"
      input_artifacts  = ["source_output"]
      name             = "${local.name_prefix}-fts-db-prod-snapshot-share"
      owner            = "AWS"
      provider         = "CodeBuild"
      version          = "1"
      output_artifacts = ["PROD_CONTEXT"]
      configuration = {
        ProjectName = aws_codebuild_project.prod_snapshot_share.name
      }
    }
  }

  stage {
    name = "Staging-Copy-Snapshot"

    action {
      category         = "Build"
      input_artifacts  = ["source_output", "PROD_CONTEXT"]
      name             = "${local.name_prefix}-fts-db-staging-copy-snapshot"
      owner            = "AWS"
      provider         = "CodeBuild"
      version          = "1"
      output_artifacts = ["STAGING_SNAPSHOT_CONTEXT"]
      configuration = {
        ProjectName   = aws_codebuild_project.staging_copy_snapshot.name
        PrimarySource = "source_output"
      }
    }
  }

  stage {
    name = "Staging-Delete-Cluster"

    action {
      category        = "Build"
      input_artifacts = ["source_output", "PROD_CONTEXT", "STAGING_SNAPSHOT_CONTEXT"]
      name            = "${local.name_prefix}-fts-db-staging-delete-cluster"
      owner           = "AWS"
      provider        = "CodeBuild"
      version         = "1"
      configuration = {
        ProjectName   = aws_codebuild_project.staging_delete_cluster.name
        PrimarySource = "source_output"
      }
    }
  }

  stage {
    name = "Staging-Restore"

    action {
      category         = "Build"
      input_artifacts  = ["source_output", "PROD_CONTEXT", "STAGING_SNAPSHOT_CONTEXT"]
      name             = "${local.name_prefix}-fts-db-staging-restore"
      owner            = "AWS"
      provider         = "CodeBuild"
      version          = "1"
      output_artifacts = ["RESTORE_CONTEXT"]
      configuration = {
        ProjectName   = aws_codebuild_project.staging_restore.name
        PrimarySource = "source_output"
      }
    }
  }

  stage {
    name = "Staging-PostRestore"

    action {
      category        = "Build"
      input_artifacts = ["source_output", "RESTORE_CONTEXT"]
      name            = "${local.name_prefix}-fts-db-staging-post-restore"
      owner           = "AWS"
      provider        = "CodeBuild"
      version         = "1"
      configuration = {
        ProjectName   = aws_codebuild_project.staging_post_restore.name
        PrimarySource = "source_output"
      }
    }
  }

  stage {
    name = "Staging-Redeploy"

    action {
      category        = "Build"
      input_artifacts = ["source_output", "RESTORE_CONTEXT"]
      name            = "${local.name_prefix}-fts-db-staging-redeploy"
      owner           = "AWS"
      provider        = "CodeBuild"
      version         = "1"
      configuration = {
        ProjectName   = aws_codebuild_project.staging_redeploy.name
        PrimarySource = "source_output"
      }
    }
  }

  tags = var.tags
}
