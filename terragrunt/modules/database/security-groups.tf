resource "aws_security_group_rule" "access_to_fts_db_import" {
  for_each = { for idx, ip in local.allowed_ips : idx => ip }

  description       = "SSH from ${each.value.comment}"
  type              = "ingress"
  from_port         = 22
  to_port           = 22
  protocol          = "tcp"
  cidr_blocks       = [each.value.value]
  security_group_id = var.ec2_sg_id
}

resource "aws_security_group_rule" "fts_db_import_to_db" {
  description              = "FTS DB Import EC2 to t5he MySQL DB"
  type                     = "egress"
  from_port                = 3306
  to_port                  = 3306
  protocol                 = "tcp"
  source_security_group_id = var.db_mysql_sg_id
  security_group_id        = var.ec2_sg_id
}

resource "aws_security_group_rule" "db_from_fts_db_import_ec2" {
  description              = "Allow MySQL from FTS DB import EC2"
  type                     = "ingress"
  from_port                = 3306
  to_port                  = 3306
  protocol                 = "tcp"
  security_group_id        = var.db_mysql_sg_id
  source_security_group_id = var.ec2_sg_id
}

resource "aws_security_group_rule" "fts_db_import_to_sirsi_db" {
  description              = "FTS DB Import EC2 to t5he Postgres DB"
  type                     = "egress"
  from_port                = 5432
  to_port                  = 5432
  protocol                 = "tcp"
  source_security_group_id = var.db_postgres_sg_id
  security_group_id        = var.ec2_sg_id
}

resource "aws_security_group_rule" "sirsi_db_from_fts_db_import_ec2" {
  description              = "Allow Postgres from FTS DB import EC2"
  type                     = "ingress"
  from_port                = 5432
  to_port                  = 5432
  protocol                 = "tcp"
  security_group_id        = var.db_postgres_sg_id
  source_security_group_id = var.ec2_sg_id
}

resource "aws_security_group_rule" "fts_db_import_to_public" {
  description       = "Temporary allow all outbound traffic for updates and installing tools"
  type              = "egress"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = var.ec2_sg_id
}
