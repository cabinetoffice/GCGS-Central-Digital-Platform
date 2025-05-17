resource "aws_efs_file_system" "cloudbeaver" {
  creation_token = "cloudbeaver-config"
  encrypted      = true

  tags = merge({Name = "cloudbeaver-access"}, var.tags)
}

resource "aws_efs_mount_target" "cloudbeaver" {
  for_each       = toset(var.private_subnet_ids)
  file_system_id = aws_efs_file_system.cloudbeaver.id
  subnet_id      = each.value
  security_groups = [var.efs_sg_id]
}

resource "aws_efs_access_point" "cloudbeaver" {
  file_system_id = aws_efs_file_system.cloudbeaver.id

  posix_user {
    uid = 0
    gid = 0
  }

  root_directory {
    path = "/${local.cloud_beaver_volume_name}"
    creation_info {
      owner_gid   = 0
      owner_uid   = 0
      permissions = "0775"
    }
  }

  tags = merge({Name = "cloudbeaver-access"}, var.tags)
}
