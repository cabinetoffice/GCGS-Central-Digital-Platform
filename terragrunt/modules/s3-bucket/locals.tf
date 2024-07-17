locals {

  readers = var.read_roles
  writers = var.write_roles

  all_roles = concat(var.read_roles, var.write_roles)
}
