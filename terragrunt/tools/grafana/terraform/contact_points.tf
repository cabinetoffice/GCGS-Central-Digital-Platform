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
- **{{ if $service }}{{ $service }}{{ else }}(unknown service){{ end }}**{{ if index .Labels "severity" }} ({{ index .Labels "severity" }}){{ end }}{{ if len .Values }} — {{ template "__text_values_list" . }}{{ end }}
  {{- if .Annotations.summary }}  
  {{ .Annotations.summary }}{{ end }}
  {{- if .Annotations.description }}  
  {{ .Annotations.description }}{{ end }}
{{- end }}
{{- end }}

{{- if gt (len .Alerts.Resolved) 0 -}}
**Resolved ({{ len .Alerts.Resolved }})**
{{- range .Alerts.Resolved }}
{{- $service := or (index .Labels "service") (index .Labels "ServiceName") (index .Labels "servicename") -}}
- **{{ if $service }}{{ $service }}{{ else }}(unknown service){{ end }}**{{ if index .Labels "severity" }} ({{ index .Labels "severity" }}){{ end }}
  {{- if .Annotations.summary }}  
  {{ .Annotations.summary }}{{ end }}
{{- end }}
{{- end }}

{{- if gt (len .Alerts.Firing) 0 -}}
{{- $a := index .Alerts.Firing 0 -}}
{{- if or $a.GeneratorURL $a.SilenceURL $a.DashboardURL $a.PanelURL }}
**Links**
{{- if $a.GeneratorURL }}  
Source: [Open]({{ $a.GeneratorURL }}){{ end }}
{{- if $a.SilenceURL }}  
Silence: [Open]({{ $a.SilenceURL }}){{ end }}
{{- if $a.DashboardURL }}  
Dashboard: [Open]({{ $a.DashboardURL }}){{ end }}
{{- if $a.PanelURL }}  
Panel: [Open]({{ $a.PanelURL }}){{ end }}
{{- end }}
{{- end }}
EOT
  }
}
