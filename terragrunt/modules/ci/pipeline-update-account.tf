resource "aws_codepipeline" "update_account" {
  name     = "${local.name_prefix}-${local.update_account_cb_name}"
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
        BranchName       = "main"
      }
    }
  }

  stage {
    name = "Build"
    action {
      name             = aws_codebuild_project.update_account.name
      category         = "Build"
      owner            = "AWS"
      provider         = "CodeBuild"
      input_artifacts  = ["source_output"]
      output_artifacts = []
      version          = "1"

      configuration = {
        ProjectName = aws_codebuild_project.update_account.name,
      }
    }
  }

  tags = var.tags
}
