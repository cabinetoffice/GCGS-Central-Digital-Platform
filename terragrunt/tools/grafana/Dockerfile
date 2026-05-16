FROM grafana/grafana:latest

USER root

RUN mkdir -p /etc/grafana/provisioning/datasources

RUN if command -v apk >/dev/null 2>&1; then \
    apk add --no-cache postgresql-client; \
  else \
    apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*; \
  fi

COPY configs/datasource-delete.yaml /etc/grafana/provisioning/datasources/datasource-delete.yaml
COPY configs/db-cleanup.sh /usr/local/bin/db-cleanup.sh
COPY configs/entrypoint.sh /entrypoint.sh
COPY configs/grafana.ini /etc/grafana/grafana.ini

RUN chmod +x /entrypoint.sh /usr/local/bin/db-cleanup.sh \
  && chown -R 472:472 /etc/grafana/provisioning /etc/grafana/grafana.ini /entrypoint.sh /usr/local/bin/db-cleanup.sh

USER grafana

ENTRYPOINT ["/entrypoint.sh"]
