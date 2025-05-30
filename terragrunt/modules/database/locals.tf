locals {
  name_prefix        = var.product.resource_name
  sirsi_cluster_name = "${local.name_prefix}-cluster"
  ev_cluster_name    = "${local.name_prefix}-ev-cluster"
  fts_cluster_name   = "${local.name_prefix}-fts-cluster"

  is_production = var.is_production || var.environment == "staging"

  fts_db_parameters_cluster = {
    character_set_database          = "latin1"
    character_set_server            = "latin1"
    collation_server                = "latin1_swedish_ci"
    explicit_defaults_for_timestamp = 0
    sql_mode                        = "NO_ENGINE_SUBSTITUTION,STRICT_TRANS_TABLES"
  }

  fts_db_parameters_instance = {
    sql_mode = "NO_ENGINE_SUBSTITUTION,STRICT_TRANS_TABLES"
  }
}
