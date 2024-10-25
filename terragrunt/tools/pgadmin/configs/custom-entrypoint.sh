#!/bin/sh

echo "Configuring pgAdmin..."

export CONFIG_DATABASE_URI="postgresql://${PGADMIN_DATABASE_USERNAME}:${PGADMIN_DATABASE_PASSWORD}@${PGADMIN_DATABASE_HOST}:5432/${PGADMIN_DATABASE_NAME}"

check_sirsi_exists() {
    PGPASSWORD=$PGADMIN_DATABASE_PASSWORD psql -h $PGADMIN_DATABASE_HOST -U $PGADMIN_DATABASE_USERNAME -d $PGADMIN_DATABASE_NAME -tAc "SELECT 1 FROM public.server WHERE name = 'SIRSI'" | grep -q 1
    return $?
}

if check_sirsi_exists; then
    echo "SIRSI server already exists. Skipping adding all servers..."
else
    echo "Adding all servers..."
    cat <<EOF > /pgadmin4/servers.json
{
    "Servers": {
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
        "3": {
            "Name": "PGAdmin",
            "Group": "Servers",
            "Host": "${PGADMIN_DATABASE_HOST}",
            "Port": 5432,
            "MaintenanceDB": "${PGADMIN_DATABASE_NAME}",
            "Username": "${PGADMIN_DATABASE_USERNAME}",
            "SSLMode": "prefer"
        }
    }
}
EOF
fi

echo "Baking CONFIG_DATABASE_URI in config_local.py:"
if [ ! -f /pgadmin4/config_local.py ]; then
    touch /pgadmin4/config_local.py
fi
echo "CONFIG_DATABASE_URI = '${CONFIG_DATABASE_URI}'" > /pgadmin4/config_local.py

echo "Configuring pgAdmin, is done!"
echo "Handing over to pgAdmin."

/entrypoint.sh "$@"
