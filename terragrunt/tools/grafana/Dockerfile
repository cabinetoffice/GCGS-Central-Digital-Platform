FROM grafana/grafana:latest

USER root

RUN mkdir -p /etc/grafana/provisioning/dashboards /var/lib/grafana/dashboards

COPY configs/datasource.yaml.tpl /etc/grafana/provisioning/datasources/datasource.yaml.tpl
COPY configs/dashboard.yaml /etc/grafana/provisioning/dashboards/dashboard.yaml
COPY configs/entrypoint.sh /entrypoint.sh
COPY configs/dashboards /var/lib/grafana/dashboards/
COPY configs/grafana.ini /etc/grafana/grafana.ini

RUN chmod +x /entrypoint.sh \
  && chown -R 472:472 /etc/grafana/provisioning /etc/grafana/grafana.ini /entrypoint.sh /var/lib/grafana/dashboards \
  && chmod -R u+rwX /var/lib/grafana/dashboards

USER grafana

ENTRYPOINT ["/entrypoint.sh"]
