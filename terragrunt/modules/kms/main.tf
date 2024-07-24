resource "aws_kms_key" "key" {
  customer_master_key_spec = var.customer_master_key_spec
  deletion_window_in_days  = var.deletion_window_in_days
  description              = var.description
  enable_key_rotation      = true
  key_usage                = var.key_usage
  tags = merge(
    {
      Name = replace(var.key_alias, "_", "-")
    },
    var.tags
  )

  # Using jsonencode here ensures the last item in the list does not have a trailing comma (,)
  policy = templatefile("${path.module}/templates/key-policy.json.tftpl", {
    account_id              = data.aws_caller_identity.current.account_id
    admin_role              = var.key_admin_role
    custom_policies         = var.custom_policies
    key_user_arns_json      = jsonencode([for arn in var.key_user_arns : arn])
    key_user_arns_length    = length(var.key_user_arns)
    other_aws_accounts      = var.other_aws_accounts
    other_aws_accounts_json = jsonencode([for env in var.other_aws_accounts : "arn:aws:iam::${env}:root"])
  })
}

resource "aws_kms_alias" "alias" {
  name          = replace("alias/${var.key_alias}", "_", "-")
  target_key_id = aws_kms_key.key.key_id
}
