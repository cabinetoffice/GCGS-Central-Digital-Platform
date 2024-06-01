resource "aws_iam_role" "terraform" {
  assume_role_policy = data.aws_iam_policy_document.terraform_assume.json
  name               = "${local.name_prefix}-${var.environment}-terraform"
  tags               = var.tags
}

resource "aws_iam_policy" "terraform" {
  name        = "${local.name_prefix}-terraform"
  description = "Terraform specific policy"
  policy      = data.aws_iam_policy_document.terraform.json
  tags        = var.tags
}

resource "aws_iam_policy" "terraform_global" {
  name        = "${local.name_prefix}-terraform-global"
  description = "Global policy"
  policy      = data.aws_iam_policy_document.terraform_global.json
  tags        = var.tags
}

resource "aws_iam_policy" "terraform_product" {
  name        = "${local.name_prefix}-terraform-product"
  description = "Product's specific policy"
  policy      = data.aws_iam_policy_document.terraform_product.json
  tags        = var.tags
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
