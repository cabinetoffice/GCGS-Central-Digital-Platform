locals {
  name_prefix                = var.product.resource_name
  sirsi_cluster_name         = "${local.name_prefix}-cluster"
  ev_cluster_name            = "${local.name_prefix}-ev-cluster"
  find_a_tender_cluster_name = "${local.name_prefix}-find-a-tender-cluster"

  is_production = var.is_production || var.environment == "staging" # @TODO(ABN) switch to use is_staging
  is_staging    = var.environment == "staging"

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
  fts_log_exports  = contains(["development", "production"], var.environment) ? ["audit", "error", "general", "slowquery"] : []

  fts_db_parameters_cluster = {
    character_set_database          = "latin1"
    character_set_server            = "latin1"
    collation_server                = "latin1_swedish_ci"
    explicit_defaults_for_timestamp = 0
    general_log                     = 1
    group_concat_max_len            = 200000
    innodb_ft_enable_stopword       = "0"
    innodb_ft_min_token_size        = 1
    local_infile                    = 1
    log_output                      = "FILE"
    log_queries_not_using_indexes   = 1
    long_query_time                 = 1
    max_allowed_packet              = "293601280"
    slow_query_log                  = 1
    sql_mode                        = "NO_ENGINE_SUBSTITUTION"
    time_zone                       = "Europe/Dublin"
  }
  fts_db_parameters_instance = {
    group_concat_max_len      = 200000
    innodb_ft_enable_stopword = "0"
    sql_mode                  = "NO_ENGINE_SUBSTITUTION"
  }

  allowed_ips = nonsensitive(jsondecode(data.aws_secretsmanager_secret_version.allowed_ips.secret_string))

  has_import_instance   = true
  import_instance_state = contains(["production"], var.environment) ? "running" : "stopped"
  import_instance_tags  = merge(var.tags, { Name = "fts-db-import" })

  fts_instance_count = contains(["development", "integration"], var.environment) ? 1 : 2
  cfs_instance_count = local.fts_instance_count

  fts_run_tokens = jsondecode(data.aws_secretsmanager_secret_version.fts_app_secrets.secret_string)
  cfs_run_tokens = jsondecode(data.aws_secretsmanager_secret_version.cfs_app_secrets.secret_string)

  fts_proxy_auth_users = {
    registrar = {
      username = "${module.cluster_fts.db_name}_registrar"
      password = local.fts_run_tokens.RUN_REGISTRAR_TOKEN
    }
    guest = {
      username = "${module.cluster_fts.db_name}_guest"
      password = local.fts_run_tokens.RUN_GUEST_TOKEN
    }
    migrator = {
      username = "${module.cluster_fts.db_name}_migrator"
      password = local.fts_run_tokens.RUN_MIGRATOR_TOKEN
    }
  }

  cfs_proxy_auth_users = {
    registrar = {
      username = "${module.cluster_cfs.db_name}_registrar"
      password = local.cfs_run_tokens.RUN_REGISTRAR_TOKEN
    }
    guest = {
      username = "${module.cluster_cfs.db_name}_guest"
      password = local.cfs_run_tokens.RUN_GUEST_TOKEN
    }
    migrator = {
      username = "${module.cluster_cfs.db_name}_migrator"
      password = local.cfs_run_tokens.RUN_MIGRATOR_TOKEN
    }
  }

  fts_proxy_auth_secret_arns = [for s in aws_secretsmanager_secret.fts_proxy_auth_user : s.arn]
  cfs_proxy_auth_secret_arns = [for s in aws_secretsmanager_secret.cfs_proxy_auth_user : s.arn]

  rds_proxy_log_retention_in_days = var.environment == "production" ? 90 : 30
}
