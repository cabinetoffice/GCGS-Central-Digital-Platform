resource "grafana_rule_group" "ecs_task_health" {
  name             = "ecs-task-health-1m"
  folder_uid       = grafana_folder.alerts.uid
  interval_seconds = 60

  rule {
    name           = "ECS tasks below desired"
    condition      = "tasks_missing_gt0"
    for            = "5m"
    no_data_state  = "OK"
    exec_err_state = "Error"
    is_paused      = false

    annotations = {}
    labels = {
      severity = "warning"
    }

    data {
      ref_id         = "desired_tasks"
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
        refId             = "desired_tasks"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Maximum"
      })
    }

    data {
      ref_id         = "running_tasks"
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
        refId             = "running_tasks"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Maximum"
      })
    }

    data {
      ref_id         = "desired_last"
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
        expression    = "desired_tasks"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "desired_last"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "running_last"
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
        expression    = "running_tasks"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "running_last"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "tasks_missing"
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
        expression    = "$desired_last - $running_last"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "tasks_missing"
        type          = "math"
      })
    }

    data {
      ref_id         = "tasks_missing_gt0"
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
        expression    = "tasks_missing"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "tasks_missing_gt0"
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
    name           = "ECS tasks pending too long"
    condition      = "pending_gt0"
    for            = "3m"
    no_data_state  = "OK"
    exec_err_state = "Error"
    is_paused      = false

    annotations = {}
    labels = {
      severity = "warning"
    }

    data {
      ref_id         = "pending_tasks"
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
        refId             = "pending_tasks"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Maximum"
      })
    }

    data {
      ref_id         = "pending_last"
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
        expression    = "pending_tasks"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "pending_last"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "pending_gt0"
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
        expression    = "pending_last"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "pending_gt0"
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
    name           = "ECS tasks stopping/restarting"
    condition      = "tasks_stopped_gt"
    for            = "5m"
    no_data_state  = "OK"
    exec_err_state = "Error"
    is_paused      = false

    annotations = {}
    labels = {
      severity = "warning"
    }

    data {
      ref_id         = "tasks_stopped"
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
        metricName        = "TaskStoppedCount"
        metricQueryType   = 0
        namespace         = "ECS/ContainerInsights"
        period            = "1m"
        queryLanguage     = "CWLI"
        queryMode         = "Metrics"
        refId             = "tasks_stopped"
        region            = "default"
        sqlExpression     = ""
        statistic         = "Sum"
      })
    }

    data {
      ref_id         = "tasks_stopped_last"
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
        expression    = "tasks_stopped"
        intervalMs    = 1000
        maxDataPoints = 43200
        reducer       = "last"
        refId         = "tasks_stopped_last"
        type          = "reduce"
      })
    }

    data {
      ref_id         = "tasks_stopped_gt"
      datasource_uid = "__expr__"
      relative_time_range {
        from = 0
        to   = 0
      }

      model = jsonencode({
        conditions = [{
          evaluator = {
            params = [var.ecs_task_stopped_threshold]
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
        expression    = "tasks_stopped_last"
        intervalMs    = 1000
        maxDataPoints = 43200
        refId         = "tasks_stopped_gt"
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
