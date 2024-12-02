locals {
  name_prefix = var.externals_product.resource_name

  db_parameters_cluster = {
    character_set_database          = "latin1"
    character_set_server            = "latin1"
    collation_server                = "latin1_swedish_ci"
    explicit_defaults_for_timestamp = 0
    sql_mode                        = "NO_ENGINE_SUBSTITUTION,STRICT_TRANS_TABLES"
  }

  db_parameters_instance = {
    sql_mode = "NO_ENGINE_SUBSTITUTION,STRICT_TRANS_TABLES"
  }

}
