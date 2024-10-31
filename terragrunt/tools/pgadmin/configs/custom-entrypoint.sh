#!/bin/sh

echo "Configuring pgAdmin..."

export CONFIG_DATABASE_URI="postgresql://${PGADMIN_DATABASE_USERNAME}:${PGADMIN_DATABASE_PASSWORD}@${PGADMIN_DATABASE_HOST}:5432/${PGADMIN_DATABASE_NAME}"

clear_all_servers() {
    echo "Removing all existing servers from pgAdmin configuration..."
    PGPASSWORD=$PGADMIN_DATABASE_PASSWORD psql -h $PGADMIN_DATABASE_HOST -U $PGADMIN_DATABASE_USERNAME -d $PGADMIN_DATABASE_NAME -c "DELETE FROM public.server;"
    PGPASSWORD=$PGADMIN_DATABASE_PASSWORD psql -h $PGADMIN_DATABASE_HOST -U $PGADMIN_DATABASE_USERNAME -d $PGADMIN_DATABASE_NAME -c "DELETE FROM public.servergroup;"
}

clear_all_servers


databases="SIRSI Entity_Verification"
db_addresses="$DB_SIRSI_ADDRESS $DB_ENTITY_VERIFICATION_ADDRESS"
db_names="$DB_SIRSI_NAME $DB_ENTITY_VERIFICATION_NAME"
additional_usernames="ali.bahman@abntech.co.uk"  # Add more usernames space separated as needed

echo "Adding all servers..."
echo "{" > /pgadmin4/servers.json
echo '    "Servers": {' >> /pgadmin4/servers.json

cat <<EOF >> /pgadmin4/servers.json
        "Admin": {
            "Name": "PGAdmin",
            "Group": "Servers",
            "Host": "${PGADMIN_DATABASE_HOST}",
            "Port": 5432,
            "MaintenanceDB": "${PGADMIN_DATABASE_NAME}",
            "Username": "${PGADMIN_DATABASE_USERNAME}",
            "SSLMode": "prefer"
        },
        "1": {
            "Name": "SIRSI",
            "Group": "CDP",
            "Host": "${DB_SIRSI_ADDRESS}",
            "Port": 5432,
            "MaintenanceDB": "${DB_SIRSI_NAME}",
            "Username": "${DB_SIRSI_USERNAME}",
            "SSLMode": "prefer"
        },
        "2": {
            "Name": "Entity Verification",
            "Group": "CDP",
            "Host": "${DB_ENTITY_VERIFICATION_ADDRESS}",
            "Port": 5432,
            "MaintenanceDB": "${DB_ENTITY_VERIFICATION_NAME}",
            "Username": "${DB_ENTITY_VERIFICATION_USERNAME}",
            "SSLMode": "prefer"
        },
EOF

server_id=3
db_index=1

for database in $databases; do
    db_address=$(echo $db_addresses | cut -d' ' -f$db_index)
    db_name=$(echo $db_names | cut -d' ' -f$db_index)

    for username in $additional_usernames; do
        cat <<EOF >> /pgadmin4/servers.json
        "${server_id}": {
            "Name": "${username} on ${database}",
            "Group": "CDP ${username}",
            "Host": "${db_address}",
            "Port": 5432,
            "MaintenanceDB": "${db_name}",
            "Username": "${username}",
            "SSLMode": "prefer"
        },
EOF
        server_id=$((server_id + 1))
    done
    db_index=$((db_index + 1))
done

sed -i '$ s/,$//' /pgadmin4/servers.json

echo "    }" >> /pgadmin4/servers.json
echo "}" >> /pgadmin4/servers.json

echo "Baking CONFIG_DATABASE_URI in config_local.py:"
if [ ! -f /pgadmin4/config_local.py ]; then
    touch /pgadmin4/config_local.py
fi
echo "CONFIG_DATABASE_URI = '${CONFIG_DATABASE_URI}'" > /pgadmin4/config_local.py

echo "Configuration of pgAdmin is complete!"
echo "Handing over to pgAdmin."

exec /entrypoint.sh "$@"
