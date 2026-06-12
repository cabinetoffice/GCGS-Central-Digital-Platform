resource "grafana_rule_group" "rds_health" {
  name             = "rds-5m"
  folder_uid       = grafana_folder.alerts.uid
  interval_seconds = 300

  rule {
    name           = "RDS CPU high"
    condition      = "cpu_gt"
    for            = "10m"
    no_data_state  = "OK"
    exec_err_state = "Error"
    is_paused      = false

    annotations = {}
    labels = {
      severity = "warning"
    }

    data {
      ref_id         = "cpu_util"
      datasource_uid = grafana_data_source.cloudwatch.uid
      relative_time_range {
        from = 600
        to   = 0
      }

      model = jsonencode({
        dimensions = {
          DBInstanceIdentifier = "*"
        }
        expression        = ""
        id                = ""
        intervalMs        = 1000
        label             = ""
        logGroups         = []
        matchExact        = true
        maxDataPoints     = 43200
        metricEditorMode  = 0
        metricName        = "CPUUtilization"
        metricQueryType   = 0
        namespace         = "AWS/RDS"
        period            = "5m"
        queryLanguage     = "CWLI"
        queryMode         = "Metrics"
        refId             = "cpu_util"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Average"
      })
    }

    data {
      ref_id         = "cpu_last"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = []
            type   = "gt"
          }
          operator = {
            type = "and"
          }
          query = {
            params = []
          }
          reducer = {
            params = []
            type   = "last"
          }
          type = "query"
        }]
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "cpu_util"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "cpu_last"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "cpu_gt"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = [var.rds_cpu_threshold]
            type   = "gt"
          }
          operator = {
            type = "and"
          }
          query = {
            params = []
          }
          reducer = {
            params = []
            type   = "last"
          }
          type = "query"
        }]
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "cpu_last"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "cpu_gt"
        type          = "threshold"
      })
    }

    dynamic "notification_settings" {
      for_each = local.teams_webhook_url != "" ? [1] : []
      content {
        contact_point = grafana_contact_point.teams[0].name
      }
    }
  }

  rule {
    name           = "RDS Free storage low"
    condition      = "free_storage_low"
    for            = "10m"
    no_data_state  = "OK"
    exec_err_state = "Error"
    is_paused      = false

    annotations = {}
    labels = {
      severity = "warning"
    }

    data {
      ref_id         = "free_storage"
      datasource_uid = grafana_data_source.cloudwatch.uid
      relative_time_range {
        from = 600
        to   = 0
      }

      model = jsonencode({
        dimensions = {
          DBInstanceIdentifier = "*"
        }
        expression        = ""
        id                = ""
        intervalMs        = 1000
        label             = ""
        logGroups         = []
        matchExact        = true
        maxDataPoints     = 43200
        metricEditorMode  = 0
        metricName        = "FreeStorageSpace"
        metricQueryType   = 0
        namespace         = "AWS/RDS"
        period            = "5m"
        queryLanguage     = "CWLI"
        queryMode         = "Metrics"
        refId             = "free_storage"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Minimum"
      })
    }

    data {
      ref_id         = "free_storage_last"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = []
            type   = "lt"
          }
          operator = {
            type = "and"
          }
          query = {
            params = []
          }
          reducer = {
            params = []
            type   = "last"
          }
          type = "query"
        }]
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "free_storage"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "free_storage_last"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "free_storage_low"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = [var.rds_free_storage_threshold_bytes]
            type   = "lt"
          }
          operator = {
            type = "and"
          }
          query = {
            params = []
          }
          reducer = {
            params = []
            type   = "last"
          }
          type = "query"
        }]
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "free_storage_last"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "free_storage_low"
        type          = "threshold"
      })
    }

    dynamic "notification_settings" {
      for_each = local.teams_webhook_url != "" ? [1] : []
      content {
        contact_point = grafana_contact_point.teams[0].name
      }
    }
  }

  rule {
    name           = "RDS Freeable memory low"
    condition      = "freeable_mem_low"
    for            = "10m"
    no_data_state  = "OK"
    exec_err_state = "Error"
    is_paused      = false

    annotations = {}
    labels = {
      severity = "warning"
    }

    data {
      ref_id         = "freeable_mem"
      datasource_uid = grafana_data_source.cloudwatch.uid
      relative_time_range {
        from = 600
        to   = 0
      }

      model = jsonencode({
        dimensions = {
          DBInstanceIdentifier = "*"
        }
        expression        = ""
        id                = ""
        intervalMs        = 1000
        label             = ""
        logGroups         = []
        matchExact        = true
        maxDataPoints     = 43200
        metricEditorMode  = 0
        metricName        = "FreeableMemory"
        metricQueryType   = 0
        namespace         = "AWS/RDS"
        period            = "5m"
        queryLanguage     = "CWLI"
        queryMode         = "Metrics"
        refId             = "freeable_mem"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Minimum"
      })
    }

    data {
      ref_id         = "freeable_mem_last"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = []
            type   = "lt"
          }
          operator = {
            type = "and"
          }
          query = {
            params = []
          }
          reducer = {
            params = []
            type   = "last"
          }
          type = "query"
        }]
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "freeable_mem"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "freeable_mem_last"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "freeable_mem_low"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = [var.rds_freeable_memory_threshold_bytes]
            type   = "lt"
          }
          operator = {
            type = "and"
          }
          query = {
            params = []
          }
          reducer = {
            params = []
            type   = "last"
          }
          type = "query"
        }]
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "freeable_mem_last"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "freeable_mem_low"
        type          = "threshold"
      })
    }

    dynamic "notification_settings" {
      for_each = local.teams_webhook_url != "" ? [1] : []
      content {
        contact_point = grafana_contact_point.teams[0].name
      }
    }
  }

  rule {
    name           = "RDS connections high"
    condition      = "db_conns_gt"
    for            = "10m"
    no_data_state  = "OK"
    exec_err_state = "Error"
    is_paused      = false

    annotations = {}
    labels = {
      severity = "warning"
    }

    data {
      ref_id         = "db_conns"
      datasource_uid = grafana_data_source.cloudwatch.uid
      relative_time_range {
        from = 600
        to   = 0
      }

      model = jsonencode({
        dimensions = {
          DBInstanceIdentifier = "*"
        }
        expression        = ""
        id                = ""
        intervalMs        = 1000
        label             = ""
        logGroups         = []
        matchExact        = true
        maxDataPoints     = 43200
        metricEditorMode  = 0
        metricName        = "DatabaseConnections"
        metricQueryType   = 0
        namespace         = "AWS/RDS"
        period            = "5m"
        queryLanguage     = "CWLI"
        queryMode         = "Metrics"
        refId             = "db_conns"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Average"
      })
    }

    data {
      ref_id         = "db_conns_last"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = []
            type   = "gt"
          }
          operator = {
            type = "and"
          }
          query = {
            params = []
          }
          reducer = {
            params = []
            type   = "last"
          }
          type = "query"
        }]
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "db_conns"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "db_conns_last"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "db_conns_gt"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = [var.rds_connections_threshold]
            type   = "gt"
          }
          operator = {
            type = "and"
          }
          query = {
            params = []
          }
          reducer = {
            params = []
            type   = "last"
          }
          type = "query"
        }]
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "db_conns_last"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "db_conns_gt"
        type          = "threshold"
      })
    }

    dynamic "notification_settings" {
      for_each = local.teams_webhook_url != "" ? [1] : []
      content {
        contact_point = grafana_contact_point.teams[0].name
      }
    }
  }

  rule {
    name           = "RDS read latency high"
    condition      = "read_latency_gt"
    for            = "10m"
    no_data_state  = "OK"
    exec_err_state = "Error"
    is_paused      = false

    annotations = {}
    labels = {
      severity = "warning"
    }

    data {
      ref_id         = "read_latency"
      datasource_uid = grafana_data_source.cloudwatch.uid
      relative_time_range {
        from = 600
        to   = 0
      }

      model = jsonencode({
        dimensions = {
          DBInstanceIdentifier = "*"
        }
        expression        = ""
        id                = ""
        intervalMs        = 1000
        label             = ""
        logGroups         = []
        matchExact        = true
        maxDataPoints     = 43200
        metricEditorMode  = 0
        metricName        = "ReadLatency"
        metricQueryType   = 0
        namespace         = "AWS/RDS"
        period            = "5m"
        queryLanguage     = "CWLI"
        queryMode         = "Metrics"
        refId             = "read_latency"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Average"
      })
    }

    data {
      ref_id         = "read_latency_last"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = []
            type   = "gt"
          }
          operator = {
            type = "and"
          }
          query = {
            params = []
          }
          reducer = {
            params = []
            type   = "last"
          }
          type = "query"
        }]
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "read_latency"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "read_latency_last"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "read_latency_gt"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = [var.rds_read_latency_threshold_seconds]
            type   = "gt"
          }
          operator = {
            type = "and"
          }
          query = {
            params = []
          }
          reducer = {
            params = []
            type   = "last"
          }
          type = "query"
        }]
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "read_latency_last"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "read_latency_gt"
        type          = "threshold"
      })
    }

    dynamic "notification_settings" {
      for_each = local.teams_webhook_url != "" ? [1] : []
      content {
        contact_point = grafana_contact_point.teams[0].name
      }
    }
  }

  rule {
    name           = "RDS write latency high"
    condition      = "write_latency_gt"
    for            = "10m"
    no_data_state  = "OK"
    exec_err_state = "Error"
    is_paused      = false

    annotations = {}
    labels = {
      severity = "warning"
    }

    data {
      ref_id         = "write_latency"
      datasource_uid = grafana_data_source.cloudwatch.uid
      relative_time_range {
        from = 600
        to   = 0
      }

      model = jsonencode({
        dimensions = {
          DBInstanceIdentifier = "*"
        }
        expression        = ""
        id                = ""
        intervalMs        = 1000
        label             = ""
        logGroups         = []
        matchExact        = true
        maxDataPoints     = 43200
        metricEditorMode  = 0
        metricName        = "WriteLatency"
        metricQueryType   = 0
        namespace         = "AWS/RDS"
        period            = "5m"
        queryLanguage     = "CWLI"
        queryMode         = "Metrics"
        refId             = "write_latency"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Average"
      })
    }

    data {
      ref_id         = "write_latency_last"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = []
            type   = "gt"
          }
          operator = {
            type = "and"
          }
          query = {
            params = []
          }
          reducer = {
            params = []
            type   = "last"
          }
          type = "query"
        }]
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "write_latency"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "write_latency_last"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "write_latency_gt"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = [var.rds_write_latency_threshold_seconds]
            type   = "gt"
          }
          operator = {
            type = "and"
          }
          query = {
            params = []
          }
          reducer = {
            params = []
            type   = "last"
          }
          type = "query"
        }]
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "write_latency_last"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "write_latency_gt"
        type          = "threshold"
      })
    }

    dynamic "notification_settings" {
      for_each = local.teams_webhook_url != "" ? [1] : []
      content {
        contact_point = grafana_contact_point.teams[0].name
      }
    }
  }

  depends_on = [
    grafana_data_source.cloudwatch,
    grafana_dashboard.dashboards,
    grafana_folder.alerts,
  ]
}
