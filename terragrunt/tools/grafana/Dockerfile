FROM grafana/grafana:latest

USER root

COPY configs/entrypoint.sh /entrypoint.sh
COPY configs/grafana.ini /etc/grafana/grafana.ini

RUN chmod +x /entrypoint.sh \
  && chown -R 472:472 /etc/grafana/grafana.ini /entrypoint.sh

USER grafana

ENTRYPOINT ["/entrypoint.sh"]
