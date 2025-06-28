data "aws_iam_policy_document" "terraform_global_ses" {

  statement {
    actions = [
      "ses:VerifyDomainIdentity",
      "ses:VerifyDomainDkim",
      "ses:SetIdentityMailFromDomain",
      "ses:DeleteIdentity",
      "ses:GetIdentityVerificationAttributes",
      "ses:GetIdentityDkimAttributes",
      "ses:UpdateIdentityPolicy",
      "ses:PutIdentityPolicy"
    ]
    effect = "Allow"
    resources = [
      "*",
    ]
    sid = "ManageSES"
  }

}
