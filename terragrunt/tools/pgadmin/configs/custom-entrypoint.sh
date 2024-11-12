#!/bin/sh

echo "Configuring pgAdmin..."

export CONFIG_DATABASE_URI="postgresql://${PGADMIN_DATABASE_USERNAME}:${PGADMIN_DATABASE_PASSWORD}@${PGADMIN_DATABASE_HOST}:5432/${PGADMIN_DATABASE_NAME}"

clear_all_servers() {
    echo "Removing all existing servers from pgAdmin configuration..."
    PGPASSWORD="$PGADMIN_DATABASE_PASSWORD" psql -h "$PGADMIN_DATABASE_HOST" -U "$PGADMIN_DATABASE_USERNAME" -d "$PGADMIN_DATABASE_NAME" -c "DELETE FROM public.server;"
    PGPASSWORD="$PGADMIN_DATABASE_PASSWORD" psql -h "$PGADMIN_DATABASE_HOST" -U "$PGADMIN_DATABASE_USERNAME" -d "$PGADMIN_DATABASE_NAME" -c "DELETE FROM public.user;"
}

add_server_to_json() {
    local id=$1
    local name=$2
    local group=$3
    local host=$4
    local port=$5
    local maintenance_db=$6
    local username=$7

    echo "    \"$id\": {" >> /pgadmin4/servers.json
    echo "      \"Name\": \"$name\"," >> /pgadmin4/servers.json
    echo "      \"Group\": \"$group\"," >> /pgadmin4/servers.json
    echo "      \"Host\": \"$host\"," >> /pgadmin4/servers.json
    echo "      \"Port\": $port," >> /pgadmin4/servers.json
    echo "      \"MaintenanceDB\": \"$maintenance_db\"," >> /pgadmin4/servers.json
    echo "      \"Username\": \"$username\"," >> /pgadmin4/servers.json
    echo "      \"SSLMode\": \"prefer\"" >> /pgadmin4/servers.json
    echo "    }," >> /pgadmin4/servers.json
}

configure_servers_json() {
    echo '{' > /pgadmin4/servers.json
    echo '  "Servers": {' >> /pgadmin4/servers.json

    echo "Adding using default servers."
    add_server_to_json "1" "PGAdmin" "Admin" "$PGADMIN_DATABASE_HOST" 5432 "$PGADMIN_DATABASE_NAME" "$PGADMIN_DATABASE_USERNAME"
    add_server_to_json "2" "SIRSI" "CDP" "$DB_SIRSI_ADDRESS" 5432 "$DB_SIRSI_NAME" "$DB_SIRSI_USERNAME"
    add_server_to_json "3" "Entity Verification" "CDP" "$DB_ENTITY_VERIFICATION_ADDRESS" 5432 "$DB_ENTITY_VERIFICATION_NAME" "$DB_ENTITY_VERIFICATION_USERNAME"

    if [ -z "$SUPPORT_USERNAMES" ]; then
        echo "No support users provided"
    else
        echo "Adding servers for support users"

        # Split the SUPPORT_USERNAMES by comma and loop over each username
        id=4  # Start ID for dynamically added servers
        for username in $(echo "$SUPPORT_USERNAMES" | tr ',' ' '); do
            add_server_to_json "$id" "$username@cdp-sirsi" "Production Support" "$DB_SIRSI_ADDRESS" 5432 "$DB_SIRSI_NAME" "$username"
            id=$((id + 1))
            add_server_to_json "$id" "$username@cdp-entity-verification" "Production Support" "$DB_ENTITY_VERIFICATION_ADDRESS" 5432 "$DB_ENTITY_VERIFICATION_NAME" "$username"
            id=$((id + 1))
        done
    fi

    sed -i '$ s/,$//' /pgadmin4/servers.json
    echo "  }" >> /pgadmin4/servers.json
    echo "}" >> /pgadmin4/servers.json
}

clear_all_servers
configure_servers_json

echo "Generated servers.json:"
cat /pgadmin4/servers.json

echo "pgAdmin configuration complete."


docker run \
  -e DB_SIRSI_ADDRESS='sirsi-address' \
  -e DB_SIRSI_NAME='sirsi-db' \
  -e DB_SIRSI_USERNAME='sirsi-username' \
  -e DB_ENTITY_VERIFICATION_ADDRESS='ev-address' \
  -e DB_ENTITY_VERIFICATION_NAME='ev-db' \
  -e DB_ENTITY_VERIFICATION_USERNAME='ev-username' \
  -e PGADMIN_DATABASE_USERNAME='test-user' \
  -e PGADMIN_DATABASE_HOST='pgadmin-address' \
  -e PGADMIN_DATABASE_NAME='pgadmin-db' \
  -e PGADMIN_DEFAULT_EMAIL='admin@example.com' \
  -e PGADMIN_DEFAULT_PASSWORD='admin' \
  -e SUPPORT_USERNAMES='ali.bahman, reza.nakhjavani' \
  cabinetoffice/cdp-pgadmin:${PINNED_PGADMIN_VERSION}
