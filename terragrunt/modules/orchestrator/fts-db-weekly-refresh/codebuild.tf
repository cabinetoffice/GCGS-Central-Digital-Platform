resource "aws_codebuild_project" "prod_snapshot_share" {
  name          = "${local.name_prefix}-fts-db-prod-snapshot-share"
  description   = "Production activities: create snapshot, copy with share KMS, share with staging"
  service_role  = var.ci_build_role_arn
  build_timeout = 180
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
      value = data.aws_region.current.region
    }

    environment_variable {
      name  = "PROD_ACCOUNT_ID"
      value = var.account_ids["production"]
    }

    environment_variable {
      name  = "STAGING_ACCOUNT_ID"
      value = var.account_ids["staging"]
    }

    environment_variable {
      name  = "PRODUCT_RESOURCE_NAME"
      value = var.product.resource_name
    }
  }

  logs_config {
    cloudwatch_logs {
      group_name  = aws_cloudwatch_log_group.weekly_refresh.name
      stream_name = "${local.name_prefix}-prod-snapshot-share"
    }
  }

  source {
    type      = "CODEPIPELINE"
    buildspec = file("${path.module}/buildspecs/01-prod-snapshot-share.yml")
  }

  vpc_config {
    vpc_id             = var.vpc_id
    security_group_ids = [var.ci_sg_id]
    subnets            = var.private_subnet_ids
  }
}

resource "aws_codebuild_project" "staging_copy_restore" {
  name          = "${local.name_prefix}-fts-db-staging-copy-restore"
  description   = "Staging activities: copy shared snapshot, restore DB (scratch or main)"
  service_role  = var.ci_build_role_arn
  build_timeout = 180
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
      value = data.aws_region.current.region
    }

    environment_variable {
      name  = "PROD_ACCOUNT_ID"
      value = var.account_ids["production"]
    }

    environment_variable {
      name  = "STAGING_ACCOUNT_ID"
      value = var.account_ids["staging"]
    }

    environment_variable {
      name  = "TG_ENVIRONMENT"
      value = "staging"
    }

    environment_variable {
      name  = "PRODUCT_RESOURCE_NAME"
      value = var.product.resource_name
    }

    environment_variable {
      name  = "FTS_REFRESH_RESTORE_MODE"
      value = var.fts_refresh_restore_mode
    }

    environment_variable {
      name  = "KEEP_SCRATCH"
      value = tostring(var.keep_scratch)
    }

    environment_variable {
      name  = "FTS_REFRESH_DELETE_MAIN_CLUSTER"
      value = tostring(var.fts_refresh_delete_main_cluster)
    }
  }

  logs_config {
    cloudwatch_logs {
      group_name  = aws_cloudwatch_log_group.weekly_refresh.name
      stream_name = "${local.name_prefix}-staging-copy-restore"
    }
  }

  source {
    type      = "CODEPIPELINE"
    buildspec = file("${path.module}/buildspecs/02-staging-copy-restore.yml")
  }

  vpc_config {
    vpc_id             = var.vpc_id
    security_group_ids = [var.ci_sg_id]
    subnets            = var.private_subnet_ids
  }
}

resource "aws_codebuild_project" "staging_post_restore" {
  name          = "${local.name_prefix}-fts-db-staging-post-restore"
  description   = "Staging post-restore: reset DB user passwords (and optionally trigger migrations/redeploys)"
  service_role  = var.ci_build_role_arn
  build_timeout = 180
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
      value = data.aws_region.current.region
    }

    environment_variable {
      name  = "STAGING_ACCOUNT_ID"
      value = var.account_ids["staging"]
    }

    environment_variable {
      name  = "PRODUCT_RESOURCE_NAME"
      value = var.product.resource_name
    }
  }

  logs_config {
    cloudwatch_logs {
      group_name  = aws_cloudwatch_log_group.weekly_refresh.name
      stream_name = "${local.name_prefix}-staging-post-restore"
    }
  }

  source {
    type      = "CODEPIPELINE"
    buildspec = file("${path.module}/buildspecs/03-staging-post-restore.yml")
  }

  vpc_config {
    vpc_id             = var.vpc_id
    security_group_ids = [var.ci_sg_id]
    subnets            = var.private_subnet_ids
  }
}

resource "aws_codebuild_project" "staging_redeploy" {
  name          = "${local.name_prefix}-fts-db-staging-redeploy"
  description   = "Staging: force ECS deployments for services that use the FTS DB (via Step Functions)"
  service_role  = var.ci_build_role_arn
  build_timeout = 180
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
      value = data.aws_region.current.region
    }

    environment_variable {
      name  = "STAGING_ACCOUNT_ID"
      value = var.account_ids["staging"]
    }
  }

  logs_config {
    cloudwatch_logs {
      group_name  = aws_cloudwatch_log_group.weekly_refresh.name
      stream_name = "${local.name_prefix}-staging-redeploy"
    }
  }

  source {
    type      = "CODEPIPELINE"
    buildspec = file("${path.module}/buildspecs/04-staging-redeploy.yml")
  }

  vpc_config {
    vpc_id             = var.vpc_id
    security_group_ids = [var.ci_sg_id]
    subnets            = var.private_subnet_ids
  }
}
