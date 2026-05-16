FROM grafana/grafana:latest

USER root

RUN mkdir -p /etc/grafana/provisioning/datasources

COPY configs/datasource.yaml.tpl /etc/grafana/provisioning/datasources/datasource.yaml.tpl
COPY configs/entrypoint.sh /entrypoint.sh
COPY configs/grafana.ini /etc/grafana/grafana.ini

RUN chmod +x /entrypoint.sh \
  && chown -R 472:472 /etc/grafana/provisioning /etc/grafana/grafana.ini /entrypoint.sh

USER grafana

ENTRYPOINT ["/entrypoint.sh"]
