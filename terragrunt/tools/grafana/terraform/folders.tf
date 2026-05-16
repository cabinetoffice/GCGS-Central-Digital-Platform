resource "grafana_folder" "application" {
  uid   = "application"
  title = "Application"
}

resource "grafana_folder" "infrastructure" {
  uid   = "infrastructure"
  title = "Infrastructure"
}

resource "grafana_folder" "overview" {
  uid   = "overview"
  title = "Overview"
}

resource "grafana_folder" "traffic" {
  uid   = "traffic"
  title = "Traffic"
}
