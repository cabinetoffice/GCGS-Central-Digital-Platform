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
{{ if gt (len .Alerts.Firing) 0 }}
**Firing ({{ len .Alerts.Firing }})**<br/>
{{ range .Alerts.Firing }}
{{ $service := or (index .Labels "service") (index .Labels "ServiceName") (index .Labels "servicename") }}
- **{{ if $service }}{{ $service }}{{ else }}(unknown service){{ end }}**{{ if index .Labels "severity" }} ({{ index .Labels "severity" }}){{ end }}{{ if len .Values }} — Value: {{ template "__text_values_list" . }}{{ end }}<br/>
{{ end }}
{{ end }}

{{ if gt (len .Alerts.Resolved) 0 }}
<br/>**Resolved ({{ len .Alerts.Resolved }})**<br/>
{{ range .Alerts.Resolved }}
{{ $service := or (index .Labels "service") (index .Labels "ServiceName") (index .Labels "servicename") }}
- **{{ if $service }}{{ $service }}{{ else }}(unknown service){{ end }}**{{ if index .Labels "severity" }} ({{ index .Labels "severity" }}){{ end }}<br/>
{{ end }}
{{ end }}

{{ if gt (len .Alerts.Firing) 0 }}
{{ $a := index .Alerts.Firing 0 }}
{{ if or $a.GeneratorURL $a.SilenceURL $a.DashboardURL $a.PanelURL }}
<br/>**Links**<br/>
{{ if $a.GeneratorURL }}Source: [Open]({{ $a.GeneratorURL }})<br/>{{ end }}
{{ if $a.SilenceURL }}Silence: [Open]({{ $a.SilenceURL }})<br/>{{ end }}
{{ if $a.DashboardURL }}Dashboard: [Open]({{ $a.DashboardURL }})<br/>{{ end }}
{{ if $a.PanelURL }}Panel: [Open]({{ $a.PanelURL }})<br/>{{ end }}
{{ end }}
{{ end }}
EOT
  }
}
