resource "grafana_contact_point" "teams" {
  count = var.teams_webhook_url != "" ? 1 : 0

  name = var.alert_contact_point_name

  teams {
    url = var.teams_webhook_url
    title = <<-EOT
{{- $env := or (index .CommonLabels "environment") (index .CommonLabels "env") (index .CommonLabels "cdp_sirsi_environment") (index .CommonLabels "ClusterName") -}}
[{{ .Status | toUpper }}] {{ .CommonLabels.alertname }}{{ if $env }} ({{ $env }}){{ end }}
EOT
    message = <<-EOT
{{- if gt (len .Alerts.Firing) 0 -}}
**Firing ({{ len .Alerts.Firing }})**
{{- range .Alerts.Firing }}
{{- $service := or (index .Labels "service") (index .Labels "ServiceName") (index .Labels "servicename") -}}
- **{{ if $service }}{{ $service }}{{ else }}(unknown service){{ end }}**{{ if index .Labels "severity" }} ({{ index .Labels "severity" }}){{ end }}
  {{- if .Annotations.summary }} — {{ .Annotations.summary }}{{ end }}
  {{- if .Annotations.description }}  
  {{ .Annotations.description }}{{ end }}
  {{- if len .Values }}  
  Value: {{ template "__text_values_list" . }}{{ end }}
  {{- if .DashboardURL }}  
  Dashboard: [Open]({{ .DashboardURL }}){{ end }}
  {{- if .PanelURL }}  
  Panel: [Open]({{ .PanelURL }}){{ end }}
  {{- if .GeneratorURL }}  
  Source: [Open]({{ .GeneratorURL }}){{ end }}
  {{- if .SilenceURL }}  
  Silence: [Open]({{ .SilenceURL }}){{ end }}
{{- end }}
{{- end }}

{{- if gt (len .Alerts.Resolved) 0 -}}
**Resolved ({{ len .Alerts.Resolved }})**
{{- range .Alerts.Resolved }}
{{- $service := or (index .Labels "service") (index .Labels "ServiceName") (index .Labels "servicename") -}}
- **{{ if $service }}{{ $service }}{{ else }}(unknown service){{ end }}**{{ if index .Labels "severity" }} ({{ index .Labels "severity" }}){{ end }}
  {{- if .Annotations.summary }} — {{ .Annotations.summary }}{{ end }}
  {{- if .GeneratorURL }}  
  Source: [Open]({{ .GeneratorURL }}){{ end }}
{{- end }}
{{- end }}
EOT
  }
}
