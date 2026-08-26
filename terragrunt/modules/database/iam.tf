resource "aws_iam_policy" "db_import" {
  name   = "${local.name_prefix}-db-import"
  policy = data.aws_iam_policy_document.db_import_handover_s3.json
}

resource "aws_iam_role_policy_attachment" "db_import_handover_s3" {
  role       = var.role_db_import_name
  policy_arn = aws_iam_policy.db_import.arn
}

resource "aws_iam_role" "rds_proxy" {
  name = "${local.name_prefix}-rds-proxy"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = "rds.amazonaws.com"
        }
        Action = "sts:AssumeRole"
      }
    ]
  })

  tags = var.tags
}

resource "aws_iam_role_policy" "rds_proxy" {
  name = "${local.name_prefix}-rds-proxy"
  role = aws_iam_role.rds_proxy.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid    = "ReadDbSecrets"
        Effect = "Allow"
        Action = [
          "secretsmanager:DescribeSecret",
          "secretsmanager:GetSecretValue"
        ]
        Resource = [
          module.cluster_fts.db_credentials_arn,
          module.cluster_cfs.db_credentials_arn,
          "arn:aws:secretsmanager:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:secret:${local.name_prefix}-fts-rds-proxy-*-credentials*",
          "arn:aws:secretsmanager:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:secret:${local.name_prefix}-cfs-rds-proxy-*-credentials*",
        ]
      },
      {
        Sid    = "DecryptSecretKms"
        Effect = "Allow"
        Action = ["kms:Decrypt"]
        Resource = ["*"]
        Condition = {
          StringEquals = {
            "kms:ViaService"    = "secretsmanager.${data.aws_region.current.region}.amazonaws.com"
            "kms:CallerAccount" = data.aws_caller_identity.current.account_id
          }
        }
      }
    ]
  })
}
