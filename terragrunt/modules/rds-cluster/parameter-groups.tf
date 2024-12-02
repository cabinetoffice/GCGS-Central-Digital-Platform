
resource "aws_db_parameter_group" "this" {
  family = var.family
  name   = "${var.db_name}-instance"

  dynamic "parameter" {
    for_each = var.db_parameters_instance
    content {
      apply_method = "pending-reboot"
      name         = parameter.key
      value        = parameter.value
    }
  }

  tags = var.tags
}


resource "aws_rds_cluster_parameter_group" "this" {
  family = var.family
  name   = "${var.db_name}-cluster"

  dynamic "parameter" {
    for_each = var.db_parameters_cluster
    content {
      apply_method = "pending-reboot"
      name         = parameter.key
      value        = parameter.value
    }
  }

  tags = var.tags
}
