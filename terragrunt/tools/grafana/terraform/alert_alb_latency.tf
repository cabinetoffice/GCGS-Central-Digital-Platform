resource "grafana_rule_group" "alb_latency" {
  name             = "alb-latency-1m"
  folder_uid       = grafana_folder.alerts.uid
  interval_seconds = 60

  rule {
    name           = "ALB target response time p95 high"
    condition      = "p95_gt"
    for            = "5m"
    no_data_state  = "OK"
    exec_err_state = "Error"
    is_paused      = false

    annotations = {}
    labels = {
      severity = "warning"
    }

    data {
      ref_id         = "p95"
      datasource_uid = grafana_data_source.cloudwatch.uid
      relative_time_range {
        from = 600
        to   = 0
      }

      model = jsonencode({
        dimensions       = {}
        expression       = ""
        id               = ""
        intervalMs       = 1000
        label            = ""
        logGroups        = []
        matchExact       = true
        maxDataPoints    = 43200
        metricEditorMode = 0
        metricName       = "TargetResponseTime"
        metricQueryType  = 0
        namespace        = "AWS/ApplicationELB"
        period           = "1m"
        queryLanguage    = "CWLI"
        queryMode        = "Metrics"
        refId            = "p95"
        region           = "default"
        sqlExpression    = "SELECT p95(TargetResponseTime) FROM \"AWS/ApplicationELB\" WHERE LoadBalancer LIKE \"app/cdp-sirsi/%\" GROUP BY LoadBalancer, TargetGroup"
        statistic        = "p95"
      })
    }

    data {
      ref_id         = "p95_last"
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
        expression    = "p95"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "p95_last"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "p95_gt"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = [var.alb_target_response_time_p95_threshold_seconds]
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
        expression    = "p95_last"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "p95_gt"
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
