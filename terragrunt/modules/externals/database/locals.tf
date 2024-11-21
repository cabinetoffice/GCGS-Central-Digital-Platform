locals {
  name_prefix = var.externals_product.resource_name

  db_parameters_cluster = {
    character_set_database = "latin1"
    character_set_server = "latin1"
    collation_server = "latin1_swedish_ci"
    explicit_defaults_for_timestamp = false
    sql_mode = "NO_ENGINE_SUBSTITUTION"
  }

}
