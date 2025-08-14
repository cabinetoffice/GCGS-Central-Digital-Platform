data "aws_iam_policy_document" "terraform_global_ses" {

  statement {
    actions = [
      "ses:CreateConfigurationSet",
      "ses:CreateConfigurationSetEventDestination",
      "ses:DeleteConfigurationSet",
      "ses:DeleteConfigurationSetEventDestination",
      "ses:DeleteIdentity",
      "ses:DeleteIdentityPolicy",
      "ses:DescribeConfigurationSet",
      "ses:GetConfigurationSetEventDestinations",
      "ses:GetIdentityDkimAttributes",
      "ses:GetIdentityMailFromDomainAttributes",
      "ses:GetIdentityPolicies",
      "ses:GetIdentityVerificationAttributes",
      "ses:PutIdentityPolicy",
      "ses:SetIdentityMailFromDomain",
      "ses:UpdateIdentityPolicy",
      "ses:VerifyDomainDkim",
      "ses:VerifyDomainIdentity",
    ]
    effect = "Allow"
    resources = [
      "*",
    ]
    sid = "ManageSES"
  }

}
