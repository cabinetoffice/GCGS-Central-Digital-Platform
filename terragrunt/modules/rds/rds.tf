resource "aws_db_subnet_group" "postgres" {
  name       = "${var.db_name}-private-subnet-group"
  subnet_ids = var.private_subnet_ids

  tags = merge(
    var.tags,
    {
      Name = "${var.db_name}-private-subnet-group"
    }
  )
}

resource "aws_db_parameter_group" "postgres" {
  name   = "${var.db_name}-${floor(var.postgres_engine_version)}"
  family = "postgres${floor(var.postgres_engine_version)}"

  parameter {
    apply_method = "pending-reboot"
    name         = "max_connections"
    value        = "100"
  }

  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_db_instance" "postgres" {
  allocated_storage                   = 20
  apply_immediately                   = true
  auto_minor_version_upgrade          = false
  backup_retention_period             = var.environment == "production" ? 14 : 0
  character_set_name                  = ""
  db_name                             = replace(var.db_name, "-", "_")
  db_subnet_group_name                = aws_db_subnet_group.postgres.name
  engine                              = "postgres"
  engine_version                      = var.postgres_engine_version
  iam_database_authentication_enabled = true
  identifier                          = var.db_name
  instance_class                      = var.postgres_instance_type
  manage_master_user_password         = true
  master_user_secret_kms_key_id       = module.kms.key_id
  multi_az                            = var.environment == "production"
  skip_final_snapshot                 = true
  storage_encrypted                   = true
  username                            = "${replace(var.db_name, "-", "_")}_user"
  vpc_security_group_ids              = [var.db_postgres_sg_id]

  depends_on = [
    aws_db_subnet_group.postgres
  ]

  tags = merge(
    var.tags,
    {
      Name = replace(var.db_name, "-", "_")
    }
  )
}
