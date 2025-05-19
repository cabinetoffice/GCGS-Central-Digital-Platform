FROM dbeaver/cloudbeaver:latest

COPY configs/teams.json /opt/cloudbeaver/workspace/GlobalConfiguration/teams.json

# Add custom entrypoint
COPY configs/custom-entrypoint.sh /custom-entrypoint.sh
RUN chmod +x /custom-entrypoint.sh

ENTRYPOINT ["/custom-entrypoint.sh"]
