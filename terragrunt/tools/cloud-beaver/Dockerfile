FROM dbeaver/cloudbeaver:latest

# Add custom entrypoint
COPY configs/custom-entrypoint.sh /custom-entrypoint.sh
RUN chmod +x /custom-entrypoint.sh

ENTRYPOINT ["/custom-entrypoint.sh"]
