resource "grafana_organization_preferences" "org" {
  home_dashboard_uid = "overview-system-performance"
  depends_on         = [grafana_dashboard.dashboards]
}
