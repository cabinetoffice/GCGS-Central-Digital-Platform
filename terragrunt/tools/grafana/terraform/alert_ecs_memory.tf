resource "grafana_rule_group" "ecs_memory_high" {
  name             = "ecs-memory-5m"
  folder_uid       = grafana_folder.alerts.uid
  interval_seconds = 300

  rule {
    name           = "ECS Memory low"
    condition      = "F"
    for            = "10m"
    no_data_state  = "OK"
    exec_err_state = "Error"
    is_paused      = false

    annotations = {}
    labels = {
      severity = "warning"
    }

    data {
      ref_id         = "A"
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
        refId             = "A"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Average"
      })
    }

    data {
      ref_id         = "B"
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
        refId             = "B"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Average"
      })
    }

    data {
      ref_id         = "C"
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
        expression    = "A"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "C"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "D"
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
        expression    = "B"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "D"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "E"
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
        expression    = "$D - $C"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "E"
        type          = "math"
      })
    }

    data {
      ref_id         = "F"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = [512]
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
        expression    = "E"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "F"
        type          = "threshold"
      })
    }

    dynamic "notification_settings" {
      for_each = var.teams_webhook_url != "" ? [1] : []
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
