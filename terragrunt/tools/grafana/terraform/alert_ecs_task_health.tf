resource "grafana_rule_group" "ecs_task_health" {
  name             = "ecs-task-health-1m"
  folder_uid       = grafana_folder.alerts.uid
  interval_seconds = 60

  rule {
    name           = "ECS tasks below desired"
    condition      = "F"
    for            = "5m"
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
        from = 300
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
        metricName        = "DesiredTaskCount"
        metricQueryType   = 0
        namespace         = "ECS/ContainerInsights"
        period            = "1m"
        queryLanguage     = "CWLI"
        queryMode         = "Metrics"
        refId             = "A"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Maximum"
      })
    }

    data {
      ref_id         = "B"
      datasource_uid = grafana_data_source.cloudwatch.uid
      relative_time_range {
        from = 300
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
        metricName        = "RunningTaskCount"
        metricQueryType   = 0
        namespace         = "ECS/ContainerInsights"
        period            = "1m"
        queryLanguage     = "CWLI"
        queryMode         = "Metrics"
        refId             = "B"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Maximum"
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
        expression    = "$C - $D"
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
            params = [0]
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

  rule {
    name           = "ECS tasks pending too long"
    condition      = "C"
    for            = "3m"
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
        from = 180
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
        metricName        = "PendingTaskCount"
        metricQueryType   = 0
        namespace         = "ECS/ContainerInsights"
        period            = "1m"
        queryLanguage     = "CWLI"
        queryMode         = "Metrics"
        refId             = "A"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Maximum"
      })
    }

    data {
      ref_id         = "B"
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
        refId         = "B"
        type          = "reduce"
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
        conditions = [{
          evaluator = {
            params = [0]
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
        expression    = "B"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "C"
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
