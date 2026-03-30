resource "aws_opensearch_domain" "this" {
  domain_name    = local.domain_name
  engine_version = var.engine_version

  cluster_config {
    instance_type          = var.instance_type
    instance_count         = var.instance_count
    zone_awareness_enabled = var.zone_awareness_enabled

    dedicated_master_enabled = var.dedicated_master_enabled
    dedicated_master_type    = var.dedicated_master_type
    dedicated_master_count   = var.dedicated_master_count

    zone_awareness_config {
      availability_zone_count = var.availability_zone_count
    }

  }

  ebs_options {
    ebs_enabled = var.ebs_enabled
    volume_type = var.ebs_volume_type
    volume_size = var.ebs_volume_size
  }

  vpc_options {
    subnet_ids         = local.private_subnet_ids
    security_group_ids = [var.opensearch_sg_id]
  }

  encrypt_at_rest {
    enabled = true
  }

  node_to_node_encryption {
    enabled = true
  }

  domain_endpoint_options {
    enforce_https       = true
    tls_security_policy = "Policy-Min-TLS-1-2-2019-07"
  }

  access_policies = data.aws_iam_policy_document.opensearch_access.json

  advanced_security_options {
    enabled                        = true
    internal_user_database_enabled = false

    master_user_options {
      master_user_arn = var.role_ecs_task_opensearch_admin_arn
    }
  }

  log_publishing_options {
    log_type                 = "INDEX_SLOW_LOGS"
    cloudwatch_log_group_arn = aws_cloudwatch_log_group.index_slow.arn
    enabled                  = true
  }

  log_publishing_options {
    log_type                 = "SEARCH_SLOW_LOGS"
    cloudwatch_log_group_arn = aws_cloudwatch_log_group.search_slow.arn
    enabled                  = true
  }

  log_publishing_options {
    log_type                 = "ES_APPLICATION_LOGS"
    cloudwatch_log_group_arn = aws_cloudwatch_log_group.es_application.arn
    enabled                  = true
  }

  dynamic "log_publishing_options" {
    for_each = var.audit_logs_enabled ? [1] : []
    content {
      log_type                 = "AUDIT_LOGS"
      cloudwatch_log_group_arn = aws_cloudwatch_log_group.audit.arn
      enabled                  = true
    }
  }

  tags = var.tags

  depends_on = [
    aws_iam_service_linked_role.opensearch,
    aws_cloudwatch_log_resource_policy.opensearch
  ]
}
