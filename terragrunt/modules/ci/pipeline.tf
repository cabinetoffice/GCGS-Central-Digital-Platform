resource "aws_codepipeline" "update_ecs_services" {
  name     = "${local.name_prefix}-update-ecs-service"
  role_arn = var.ci_pipeline_role_arn

  artifact_store {
    location = module.s3_bucket.bucket
    type     = "S3"

    encryption_key {
      id   = aws_kms_alias.pipeline_bucket.arn
      type = "KMS"
    }
  }

  stage {
    name = "Source"
    action {
      name             = "Source"
      category         = "Source"
      owner            = "AWS"
      provider         = "CodeStarSourceConnection"
      version          = "1"
      output_artifacts = ["source_output"]
      configuration = {
        ConnectionArn    = data.aws_codestarconnections_connection.cabinet_office.arn
        FullRepositoryId = "cabinetoffice/GCGS-Central-Digital-Platform"
        BranchName       = "DP-187-CI-CD"
      }
    }
  }

  stage {
    name = "Build"
    action {
      name     = "update-ecs-service"
      category = "Build"
      owner    = "AWS"
      provider = "CodeBuild"
      input_artifacts = ["source_output"]
      output_artifacts = []
      version  = "1"

      configuration = {
        ProjectName = "${local.name_prefix}-update-ecs-service",
      }
    }
  }

  tags = var.tags
}
