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

resource "aws_db_instance" "this" {
  allocated_storage                   = 20
  apply_immediately                   = true
  auto_minor_version_upgrade          = false
  backup_retention_period             = var.backup_retention_period
  character_set_name                  = ""
  copy_tags_to_snapshot               = var.copy_tags_to_snapshot
  db_name                             = replace(var.db_name, "-", "_")
  db_subnet_group_name                = aws_db_subnet_group.this.name
  deletion_protection                 = var.deletion_protection
  engine                              = var.engine
  engine_version                      = var.engine_version
  iam_database_authentication_enabled = true
  identifier                          = var.db_name
  instance_class                      = var.instance_type
  max_allocated_storage               = var.max_allocated_storage
  manage_master_user_password         = true
  master_user_secret_kms_key_id       = module.kms.key_id
  monitoring_interval                 = var.monitoring_interval
  monitoring_role_arn                 = var.monitoring_role_arn
  multi_az                            = var.multi_az
  performance_insights_enabled        = var.performance_insights_enabled
  publicly_accessible                 = var.publicly_accessible
  skip_final_snapshot                 = true
  storage_encrypted                   = true
  storage_type                        = var.storage_type
  username                            = "${replace(var.db_name, "-", "_")}_user"
  vpc_security_group_ids              = [var.db_sg_id]

  depends_on = [
    aws_db_subnet_group.this
  ]

  tags = merge(
    var.tags,
    {
      Name = replace(var.db_name, "-", "_")
    }
  )
}

resource "aws_db_instance" "replica" {
  # @TODO(ABN) decide if read replica is wanted for this project considering its limitation on Postgres
  #            https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/USER_PostgreSQL.Replication.ReadReplicas.html#USER_PostgreSQL.Replication.ReadReplicas.Limitations
  # count                  = var.create_read_replica ? 1 : 0
  count = 0

  auto_minor_version_upgrade  = false
  instance_class              = var.instance_type
  manage_master_user_password = false
  publicly_accessible         = false
  replicate_source_db         = aws_db_instance.this.identifier

  tags = merge(
    var.tags,
    {
      Name = "${replace(var.db_name, "-", "_")}-replica"
    }
  )
}
