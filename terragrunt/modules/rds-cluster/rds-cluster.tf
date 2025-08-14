resource "aws_rds_cluster" "this" {
  apply_immediately                = var.apply_immediately
  backup_retention_period          = var.backup_retention_period
  cluster_identifier               = var.db_name
  copy_tags_to_snapshot            = var.copy_tags_to_snapshot
  database_name                    = replace(var.db_name, "-", "_")
  db_cluster_parameter_group_name  = aws_rds_cluster_parameter_group.this.name
  db_instance_parameter_group_name = aws_db_parameter_group.this.name
  db_subnet_group_name             = length(var.public_subnet_ids) > 0 ? aws_db_subnet_group.public[0].name : aws_db_subnet_group.this.name
  deletion_protection              = var.deletion_protection
  engine                           = var.engine
  engine_version                   = var.engine_version
  # kms_key_id                       = module.storage_encryption_key.key_arn
  master_password   = jsondecode(data.aws_secretsmanager_secret_version.fetched_password.secret_string)["password"]
  master_username   = "${replace(var.db_name, "-", "_")}_user"
  storage_encrypted = true

  vpc_security_group_ids = [var.db_sg_id]

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

resource "aws_db_subnet_group" "public" { # @TODO (ABN) burn me once migration is done
  count = length(var.public_subnet_ids) > 0 ? 1 : 0

  name       = "${var.db_name}-public-subnet-group"
  subnet_ids = var.public_subnet_ids

  tags = merge(
    var.tags,
    {
      Name = "${var.db_name}-public-subnet-group"
    }
  )
}

resource "aws_rds_cluster_instance" "this" {
  count = var.instance_count

  apply_immediately            = var.apply_immediately
  cluster_identifier           = aws_rds_cluster.this.id
  db_parameter_group_name      = aws_db_parameter_group.this.name
  engine                       = var.engine
  engine_version               = var.engine_version
  identifier                   = "${var.db_name}-${count.index}"
  instance_class               = var.instance_type
  monitoring_interval          = var.monitoring_interval
  monitoring_role_arn          = var.monitoring_role_arn
  performance_insights_enabled = var.performance_insights_enabled
  publicly_accessible          = var.publicly_accessible

  tags = merge(
    var.tags,
    {
      Name = "${replace(var.db_name, "-", "_")}-${count.index}"
    }
  )
}
