FROM grafana/grafana:latest

COPY configs/datasource.yaml.tpl /etc/grafana/provisioning/datasources/datasource.yaml.tpl
COPY configs/dashboard.yaml /etc/grafana/provisioning/dashboards/dashboard.yaml


COPY configs/entrypoint.sh /entrypoint.sh

RUN mkdir -p /etc/grafana/provisioning/dashboards
COPY configs/dashboards /etc/grafana/provisioning/dashboards/

ENTRYPOINT ["/entrypoint.sh"]
