resource "grafana_folder" "application" {
  title = "Application"
}

resource "grafana_folder" "infrastructure" {
  title = "Infrastructure"
}

resource "grafana_folder" "overview" {
  title = "Overview"
}

resource "grafana_folder" "traffic" {
  title = "Traffic"
}
