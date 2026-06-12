resource "grafana_rule_group" "ecs_memory_high" {
  name             = "ecs-memory-5m"
  folder_uid       = grafana_folder.alerts.uid
  interval_seconds = 300

  rule {
    name           = "ECS Memory low"
    condition      = "mem_free_low"
    for            = "10m"
    no_data_state  = "OK"
    exec_err_state = "Error"
    is_paused      = false

    annotations = {}
    labels = {
      severity = "warning"
    }

    data {
      ref_id         = "mem_used"
      datasource_uid = grafana_data_source.cloudwatch.uid
      relative_time_range {
        from = 600
        to   = 0
      }

      model = jsonencode({
        dimensions = {
          ClusterName = "*"
          ServiceName = "*"
        }
        expression        = ""
        id                = ""
        intervalMs        = 1000
        label             = ""
        logGroups         = []
        matchExact        = true
        maxDataPoints     = 43200
        metricEditorMode  = 0
        metricName        = "MemoryUtilized"
        metricQueryType   = 0
        namespace         = "ECS/ContainerInsights"
        period            = "5m"
        queryLanguage     = "CWLI"
        queryMode         = "Metrics"
        refId             = "mem_used"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Average"
      })
    }

    data {
      ref_id         = "mem_reserved"
      datasource_uid = grafana_data_source.cloudwatch.uid
      relative_time_range {
        from = 600
        to   = 0
      }

      model = jsonencode({
        dimensions = {
          ClusterName = "*"
          ServiceName = "*"
        }
        expression        = ""
        id                = ""
        intervalMs        = 1000
        label             = ""
        logGroups         = []
        matchExact        = true
        maxDataPoints     = 43200
        metricEditorMode  = 0
        metricName        = "MemoryReserved"
        metricQueryType   = 0
        namespace         = "ECS/ContainerInsights"
        period            = "5m"
        queryLanguage     = "CWLI"
        queryMode         = "Metrics"
        refId             = "mem_reserved"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Average"
      })
    }

    data {
      ref_id         = "mem_used_last"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "mem_used"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "mem_used_last"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "mem_reserved_last"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "mem_reserved"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "mem_reserved_last"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "mem_free_pct"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        datasource = {
          type = "__expr__"
          uid  = "__expr__"
        }
        expression    = "($mem_reserved_last - $mem_used_last) / $mem_reserved_last * 100"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "mem_free_pct"
        type          = "math"
      })
    }

    data {
      ref_id         = "mem_free_low"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = [20]
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
        expression    = "mem_free_pct"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "mem_free_low"
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
