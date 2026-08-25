data "aws_iam_policy_document" "terraform_product_data" {

  statement {
    actions = [
      "elasticache:AddTagsToResource",
      "elasticache:CreateCacheParameterGroup",
      "elasticache:CreateCacheSubnetGroup",
      "elasticache:CreateReplicationGroup",
      "elasticache:DeleteCacheParameterGroup",
      "elasticache:DeleteCacheSubnetGroup",
      "elasticache:DeleteReplicationGroup",
      "elasticache:DescribeCacheClusters",
      "elasticache:DescribeCacheParameterGroups",
      "elasticache:DescribeCacheParameters",
      "elasticache:DescribeCacheSubnetGroups",
      "elasticache:DescribeReplicationGroups",
      "elasticache:ListTagsForResource",
      "elasticache:ModifyReplicationGroup",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:elasticache:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:cluster:${local.name_prefix}-*",
      "arn:aws:elasticache:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:parametergroup:${local.name_prefix}-*",
      "arn:aws:elasticache:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:replicationgroup:${local.name_prefix}-*",
      "arn:aws:elasticache:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:subnetgroup:${local.name_prefix}-*",
    ]
    sid = "ManageProductCache"
  }

  statement {
    actions = [
      "rds:AddTagsToResource",
      "rds:Create*",
      "rds:Delete*",
      "rds:Describe*",
      "rds:List*",
      "rds:Modify*",
      "rds:ModifyDBCluster",
      "rds:RemoveTagsFromResource",
      "rds:ResetDBClusterParameterGroup",
      "rds:ResetDBParameterGroup",
      "rds:RestoreDBClusterFromSnapshot",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:rds:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:cluster-pg:cdp-*",
      "arn:aws:rds:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:cluster:cdp-*",
      "arn:aws:rds:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:db-proxy:cdp-*",
      "arn:aws:rds:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:db-proxy:*",
      "arn:aws:rds:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:db:cdp-*",
      "arn:aws:rds:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:pg:cdp-*",
      "arn:aws:rds:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:subgrp:cdp-*",
      "arn:aws:rds:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:target-group:cdp-*",
      "arn:aws:rds:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:target-group:*",
    ]
    sid = "ManageProductRDS"
  }

  statement {
    actions = [
      "rds:CreateDBProxy",
      "rds:CreateDBProxyEndpoint",
    ]
    effect = "Allow"
    resources = ["*"]
    condition {
      test     = "StringEquals"
      variable = "aws:RequestTag/managed_by"
      values   = ["terragrunt"]
    }
    condition {
      test     = "StringEquals"
      variable = "aws:RequestTag/component"
      values   = ["database"]
    }
    sid = "CreateDBProxyOnAllResources"
  }

  statement {
    actions = [
      "rds:RegisterDBProxyTargets",
      "rds:DeregisterDBProxyTargets",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:rds:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:cluster:cdp-*",
      "arn:aws:rds:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:db-proxy:*",
      "arn:aws:rds:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:target-group:*",
    ]
    sid = "ManageRDSProxyTargets"
  }

  statement {
    actions = [
      "iam:PassRole",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/${local.name_prefix}-rds-proxy",
    ]
    condition {
      test     = "StringEquals"
      variable = "iam:PassedToService"
      values   = ["rds.amazonaws.com"]
    }
    sid = "PassRoleToRDSProxy"
  }

  statement {
    actions = [
      "rds:AddTagsToResource",
      "rds:RemoveTagsFromResource",
      "rds:ListTagsForResource",
    ]
    effect = "Allow"
    resources = ["*"]
    sid = "TagRDSProxyResources"
  }

  statement {
    actions = [
      "rds:DescribeDBProxies",
      "rds:DescribeDBProxyEndpoints",
      "rds:DescribeDBProxyTargetGroups",
      "rds:DescribeDBProxyTargets",
    ]
    effect = "Allow"
    resources = ["*"]
    sid = "DescribeRDSProxy"
  }

  statement {
    actions = [
      "s3:Create*",
      "s3:Delete*",
      "s3:Get*",
      "s3:List*",
      "s3:Put*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:s3:::${local.name_prefix}-*",
      "arn:aws:s3:::${local.name_prefix}-*/*"
    ]
    sid = "ManageProductS3Buckets"
  }

  statement {
    actions = [
      "sqs:*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:sqs:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:${local.name_prefix}-*"
    ]
    sid = "ManageProductSQS"
  }

  statement {
    actions = [
      "es:Add*",
      "es:Create*",
      "es:Describe*",
      "es:Delete*",
      "es:List*",
      "es:Update*",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:es:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:domain/${local.name_prefix}",
    ]
    sid = "ManageProductOpenSearch"
  }

  statement {
    actions = [
      "secretsmanager:DescribeSecret",
      "secretsmanager:GetResourcePolicy",
      "secretsmanager:GetSecretValue",
    ]
    effect = "Allow"
    resources = [
      "arn:aws:secretsmanager:${data.aws_region.current.region}:${data.aws_caller_identity.current.account_id}:secret:${local.name_prefix}-grafana-alerting-webhook*",
    ]
    sid = "ReadGrafanaAlertingWebhookSecret"
  }

}
