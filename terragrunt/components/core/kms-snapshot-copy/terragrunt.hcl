terraform {
  source = local.global_vars.locals.environment == "production" ? "../../../modules//kms" : null
}

include {
  path = find_in_parent_folders("root.hcl")
}

locals {
  global_vars = read_terragrunt_config(find_in_parent_folders("root.hcl"))
  core_vars   = read_terragrunt_config(find_in_parent_folders("core.hcl"))

  tags = merge(
    local.global_vars.inputs.tags,
    local.core_vars.inputs.tags,
    {
      component = "kms-snapshot-copy"
    }
  )

  staging_terraform_role_arn = "arn:aws:iam::${local.global_vars.locals.account_ids["staging"]}:role/${local.global_vars.locals.product.resource_name}-staging-terraform"
  rds_via_service            = "rds.eu-west-2.amazonaws.com"
}

dependency "core_iam" {
  config_path = "../iam"
  mock_outputs = {
    terraform_arn = "mock"
  }
}

inputs = {
  custom_policies = [
    jsonencode({
      Sid       = "AllowStagingTerraformUseForRDSCopy"
      Effect    = "Allow"
      Principal = { AWS = local.staging_terraform_role_arn }
      Action = [
        "kms:DescribeKey",
        "kms:Decrypt",
        "kms:ReEncryptFrom",
        "kms:GenerateDataKeyWithoutPlaintext"
      ]
      Resource  = "*"
      Condition = { StringEquals = { "kms:ViaService" = local.rds_via_service } }
    }),
    jsonencode({
      Sid       = "AllowStagingTerraformCreateGrantForRDSCopy"
      Effect    = "Allow"
      Principal = { AWS = local.staging_terraform_role_arn }
      Action    = "kms:CreateGrant"
      Resource  = "*"
      Condition = {
        StringEquals = { "kms:ViaService" = local.rds_via_service }
        Bool         = { "kms:GrantIsForAWSResource" = "true" }
      }
    })
  ]

  customer_master_key_spec = "SYMMETRIC_DEFAULT"
  deletion_window_in_days  = 7
  description              = "SIRSI RDS snapshot copy key (production -> staging)"
  key_admin_role           = dependency.core_iam.outputs.terraform_arn
  key_alias                = "rds-snapshot-copy-cdp-sirsi-production"
  key_usage                = "ENCRYPT_DECRYPT"
  key_user_arns            = []
  other_aws_accounts       = {}
  tags                     = local.tags
}
