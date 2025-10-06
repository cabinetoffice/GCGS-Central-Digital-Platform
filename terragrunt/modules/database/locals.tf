locals {
  name_prefix        = var.product.resource_name
  sirsi_cluster_name = "${local.name_prefix}-cluster"
  ev_cluster_name    = "${local.name_prefix}-ev-cluster"

  is_production = var.is_production || var.environment == "staging"

  cfs_cluster_name = "${local.name_prefix}-cfs-cluster"
  cfs_db_parameters_cluster = {
    character_set_database          = "latin1"
    character_set_server            = "latin1"
    collation_server                = "latin1_swedish_ci"
    explicit_defaults_for_timestamp = 0
    group_concat_max_len            = 200000
    innodb_ft_enable_stopword       = "0"
    innodb_ft_min_token_size        = 1
    local_infile                    = 1
    max_allowed_packet              = "293601280"
    sql_mode                        = "NO_ENGINE_SUBSTITUTION"
    time_zone                       = "Europe/Dublin"
  }
  cfs_db_parameters_instance = {
    group_concat_max_len      = 200000
    innodb_ft_enable_stopword = "0"
    sql_mode                  = "NO_ENGINE_SUBSTITUTION"
  }

  fts_cluster_name = "${local.name_prefix}-fts-cluster"
  fts_db_parameters_cluster = {
    character_set_database          = "latin1"
    character_set_server            = "latin1"
    collation_server                = "latin1_swedish_ci"
    explicit_defaults_for_timestamp = 0
    group_concat_max_len            = 200000
    innodb_ft_enable_stopword       = "0"
    innodb_ft_min_token_size        = 1
    local_infile                    = 1
    max_allowed_packet              = "293601280"
    sql_mode                        = "NO_ENGINE_SUBSTITUTION"
    time_zone                       = "Europe/Dublin"
  }
  fts_db_parameters_instance = {
    group_concat_max_len      = 200000
    innodb_ft_enable_stopword = "0"
    sql_mode                  = "NO_ENGINE_SUBSTITUTION"
  }

  allowed_ips = nonsensitive(jsondecode(data.aws_secretsmanager_secret_version.allowed_ips.secret_string))

  has_import_instance  = true
  import_instance_tags = merge(var.tags, { Name = "fts-db-import" })

  fts_instance_count = contains(["development", "staging", "integration"], var.environment) ? 1 : 2
  cfs_instance_count = local.fts_instance_count
}
