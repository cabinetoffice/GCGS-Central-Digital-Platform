# Use ARG to allow dynamic versioning
ARG PGADMIN_VERSION=latest

# Use pgAdmin base image
FROM dpage/pgadmin4:${PGADMIN_VERSION}

# Switch to root to install the necessary dependencies
USER root

# Install necessary dependencies for psycopg2 using apk (for Alpine-based images)
RUN apk update && apk add --no-cache \
    build-base \
    postgresql-dev \
    python3-dev \
    py3-psycopg2

# Copy the custom entrypoint script
COPY --chmod=755 configs/custom-entrypoint.sh /custom-entrypoint.sh

# Adjust permissions for pgAdmin files
RUN chmod -R 777 /pgadmin4

# Switch back to pgadmin user
USER pgadmin

# Set the custom entrypoint
ENTRYPOINT ["/custom-entrypoint.sh"]
