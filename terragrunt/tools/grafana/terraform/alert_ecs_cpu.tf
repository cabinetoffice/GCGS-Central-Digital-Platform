resource "grafana_rule_group" "ecs_cpu_high" {
  name             = "ecs-cpu-5m"
  folder_uid       = grafana_folder.alerts.uid
  interval_seconds = 300

  rule {
    name           = "ECS CPU high"
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
        metricName        = "CpuUtilized"
        metricQueryType   = 0
        namespace         = "ECS/ContainerInsights"
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
            params = [var.ecs_cpu_threshold]
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

  depends_on = [
    grafana_data_source.cloudwatch,
    grafana_dashboard.dashboards,
    grafana_folder.alerts,
  ]
}
