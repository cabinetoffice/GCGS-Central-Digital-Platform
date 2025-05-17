resource "aws_security_group" "efs" {
  name        = "efs-sg"
  description = "Allow NFS access from ECS tasks"
  vpc_id      = var.vpc_id
}