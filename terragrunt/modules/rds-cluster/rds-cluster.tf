# Aurora MySQL Module

resource "aws_rds_cluster" "this" {
  cluster_identifier      = var.db_name
  engine                  = var.engine
  engine_version          = var.engine_version
  database_name           = replace(var.db_name, "-", "_")
  master_username         = "${replace(var.db_name, "-", "_")}_user"
  master_password         = jsondecode(aws_secretsmanager_secret_version.master_user_credentials.secret_string)["password"]
  backup_retention_period = var.backup_retention_period
  db_subnet_group_name    = aws_db_subnet_group.this.name
  vpc_security_group_ids  = [var.db_sg_id]
  deletion_protection     = var.deletion_protection
  copy_tags_to_snapshot   = var.copy_tags_to_snapshot
  tags = merge(
    var.tags,
    {
      Name = replace(var.db_name, "-", "_")
    }
  )
}

resource "aws_db_subnet_group" "this" {
  name       = "${var.db_name}-private-subnet-group"
  subnet_ids = var.private_subnet_ids

  tags = merge(
    var.tags,
    {
      Name = "${var.db_name}-private-subnet-group"
    }
  )
}

resource "aws_rds_cluster_instance" "this" {
  count                        = var.instance_count
  identifier                   = "${var.db_name}-${count.index}"
  engine                       = var.engine
  cluster_identifier           = aws_rds_cluster.this.id
  instance_class               = var.instance_type
  publicly_accessible          = var.publicly_accessible
  monitoring_interval          = var.monitoring_interval
  monitoring_role_arn          = var.monitoring_role_arn
  performance_insights_enabled = var.performance_insights_enabled
  tags = merge(
    var.tags,
    {
      Name = "${replace(var.db_name, "-", "_")}-${count.index}"
    }
  )
}

resource "aws_db_parameter_group" "this" {
  name   = var.parameter_group_name
  family = var.family

  parameter {
    apply_method = "pending-reboot"
    name         = "max_connections"
    value        = "100"
  }

  lifecycle {
    create_before_destroy = true
  }
}


