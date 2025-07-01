locals {
  all_roles = concat(var.read_roles, var.write_roles)
}
