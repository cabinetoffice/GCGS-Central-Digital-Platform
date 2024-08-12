import {
  to = aws_iam_role.ci_build
  id = local.codebuild_iam_name
}

resource "aws_iam_role" "ci_build" {
  assume_role_policy = data.aws_iam_policy_document.ci_codebuild_assume_role_policy.json
  name               = local.codebuild_iam_name
  tags               = var.tags
}

import {
  to = aws_iam_policy.ci_build_generic
  id = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:policy/${local.codebuild_iam_name}-codebuild-generic-policy"
}

resource "aws_iam_policy" "ci_build_generic" {
  name   = "${local.codebuild_iam_name}-codebuild-generic-policy"
  policy = data.aws_iam_policy_document.ci_build_generic.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "generic_codebuild_policy" {
  policy_arn = aws_iam_policy.ci_build_generic.arn
  role       = aws_iam_role.ci_build.name
}

resource "aws_iam_role_policy_attachment" "terraform_codebuild_policy" {
  policy_arn = aws_iam_policy.terraform.arn
  role       = aws_iam_role.ci_build.name
}

resource "aws_iam_role_policy_attachment" "terraform_global_codebuild_policy" {
  policy_arn = aws_iam_policy.terraform_global.arn
  role       = aws_iam_role.ci_build.name
}

resource "aws_iam_role_policy_attachment" "terraform_product_codebuild_policy" {
  policy_arn = aws_iam_policy.terraform_product.arn
  role       = aws_iam_role.ci_build.name
}

import {
  to = aws_iam_role.ci_pipeline
  id = local.pipeline_iam_name
}

resource "aws_iam_role" "ci_pipeline" {
  assume_role_policy = data.aws_iam_policy_document.ci_pipeline_assume_role_policy.json
  name               = local.pipeline_iam_name
  tags               = var.tags
}

import {
  to = aws_iam_policy.ci_pipeline_generic
  id = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:policy/${local.pipeline_iam_name}-pipeline-generic-policy"
}

resource "aws_iam_policy" "ci_pipeline_generic" {
  name   = "${local.pipeline_iam_name}-pipeline-generic-policy"
  policy = data.aws_iam_policy_document.ci_build_generic.json
  tags   = var.tags
}

resource "aws_iam_role_policy_attachment" "generic_pipeline_policy" {
  policy_arn = aws_iam_policy.ci_pipeline_generic.arn
  role       = aws_iam_role.ci_pipeline.name
}

# resource "aws_iam_role_policy_attachment" "terraform_pipeline_policy" {
#   policy_arn = aws_iam_policy.terraform.arn
#   role       = aws_iam_role.ci_pipeline.name
# }

resource "aws_iam_role_policy_attachment" "terraform_global_pipeline_policy" {
  policy_arn = aws_iam_policy.terraform_global.arn
  role       = aws_iam_role.ci_pipeline.name
}

resource "aws_iam_role_policy_attachment" "terraform_product_global_pipeline_policy" {
  policy_arn = aws_iam_policy.terraform_product.arn
  role       = aws_iam_role.ci_pipeline.name
}
