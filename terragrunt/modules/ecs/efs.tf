resource "aws_efs_file_system" "fluentbit" {
  creation_token = "fluentbit-config"
  encrypted      = true

  throughput_mode = "elastic"

  lifecycle_policy {
    transition_to_ia      = "AFTER_30_DAYS"
  }

  lifecycle_policy {
    transition_to_archive = "AFTER_90_DAYS"
  }

  tags = merge({ Name = "fluentbit" }, var.tags)
}

resource "aws_efs_mount_target" "fluentbit" {
  for_each        = toset(var.private_subnet_ids)
  file_system_id  = aws_efs_file_system.fluentbit.id
  subnet_id       = each.value
  security_groups = [var.efs_sg_id]
}

resource "aws_efs_access_point" "fluentbit" {
  file_system_id = aws_efs_file_system.fluentbit.id

  posix_user {
    uid = 0
    gid = 0
  }

  root_directory {
    path = "/${local.fluentbit_volume_name}"
    creation_info {
      owner_gid   = 0
      owner_uid   = 0
      permissions = "0775"
    }
  }

  tags = merge({ Name = "fluentbit-access" }, var.tags)
}
