FROM dpage/pgadmin4:latest

COPY --chmod=755 configs/custom-entrypoint.sh /custom-entrypoint.sh
USER root
RUN chmod -R 777 /pgadmin4
USER pgadmin

ENTRYPOINT ["/custom-entrypoint.sh"]
