# Note!
# Resources in this file are shared with orchestrator/iam module

import {
  to = aws_iam_role.terraform
  id = "${local.name_prefix}-${var.environment}-terraform"
}

resource "aws_iam_role" "terraform" {
  assume_role_policy = data.aws_iam_policy_document.terraform_assume.json
  name               = "${local.name_prefix}-${var.environment}-terraform"
  tags               = var.tags
}

import {
  to = aws_iam_policy.terraform
  id = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:policy/${local.name_prefix}-terraform"
}

resource "aws_iam_policy" "terraform" {
  name        = "${local.name_prefix}-terraform"
  description = "Terraform specific policy"
  policy      = data.aws_iam_policy_document.terraform.json
  tags        = var.tags
}

import {
  to = aws_iam_policy.terraform_global
  id = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:policy/${local.name_prefix}-terraform-global"
}

resource "aws_iam_policy" "terraform_global" {
  name        = "${local.name_prefix}-terraform-global"
  description = "Global policy"
  policy      = data.aws_iam_policy_document.terraform_global.json
  tags        = var.tags
}

import {
  to = aws_iam_policy.terraform_product
  id = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:policy/${local.name_prefix}-terraform-product"
}

resource "aws_iam_policy" "terraform_product" {
  name        = "${local.name_prefix}-terraform-product"
  description = "Product's specific policy"
  policy      = data.aws_iam_policy_document.terraform_product.json
  tags        = var.tags
}

resource "aws_iam_policy" "terraform_product_data" {
  name        = "${local.name_prefix}-terraform-product-data"
  description = "Product's Data specific policy"
  policy      = data.aws_iam_policy_document.terraform_product_data.json
  tags        = var.tags
}

import {
  to = aws_iam_policy.terraform_assume_orchestrator_role
  id = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:policy/${local.name_prefix}-terraform-assume-orchestrator-role"
}

resource "aws_iam_policy" "terraform_assume_orchestrator_role" {
  name        = "${local.name_prefix}-terraform-assume-orchestrator-role"
  description = "Policy to allow assuming the orchestrator role"
  policy      = data.aws_iam_policy_document.terraform_assume_orchestrator_role.json
}

resource "aws_iam_role_policy_attachment" "terraform" {
  policy_arn = aws_iam_policy.terraform.arn
  role       = aws_iam_role.terraform.name
}

resource "aws_iam_role_policy_attachment" "terraform_global" {
  policy_arn = aws_iam_policy.terraform_global.arn
  role       = aws_iam_role.terraform.name
}

resource "aws_iam_role_policy_attachment" "terraform_production" {
  policy_arn = aws_iam_policy.terraform_product.arn
  role       = aws_iam_role.terraform.name
}

resource "aws_iam_role_policy_attachment" "terraform_production_data" {
  policy_arn = aws_iam_policy.terraform_product_data.arn
  role       = aws_iam_role.terraform.name
}

resource "aws_iam_role_policy_attachment" "terraform_assume_orchestrator_role" {
  role       = aws_iam_role.terraform.name
  policy_arn = aws_iam_policy.terraform_assume_orchestrator_role.arn
}
