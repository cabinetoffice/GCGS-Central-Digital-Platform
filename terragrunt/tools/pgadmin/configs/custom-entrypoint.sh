#!/bin/sh

echo "Configuring pgAdmin..."

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
            "Host": "${PGADMIN_CONFIG_DATABASE_HOST}",
            "Port": 5432,
            "MaintenanceDB": "${PGADMIN_CONFIG_DATABASE_NAME}",
            "Username": "${PGADMIN_CONFIG_DATABASE_USERNAME}",
            "SSLMode": "prefer"
        }
    }
}
EOF

echo "Bake CONFIG_DATABASE_URI in config_local.py:"
export CONFIG_DATABASE_URI="postgresql://${PGADMIN_CONFIG_DATABASE_USERNAME}:${PGADMIN_CONFIG_DATABASE_PASSWORD}@${PGADMIN_CONFIG_DATABASE_HOST}:5432/${PGADMIN_CONFIG_DATABASE_NAME}"
if [ ! -f /pgadmin4/config_local.py ]; then
    touch /pgadmin4/config_local.py
fi
echo "CONFIG_DATABASE_URI = '${CONFIG_DATABASE_URI}'" > /pgadmin4/config_local.py


export PGADMIN_CONFIG_DATABASE_PASSWORD="\"$PGADMIN_CONFIG_DATABASE_PASSWORD\""
export PGADMIN_CONFIG_DATABASE_USERNAME="\"$PGADMIN_CONFIG_DATABASE_USERNAME\""
export PGADMIN_CONFIG_DATABASE_HOST="\"$PGADMIN_CONFIG_DATABASE_HOST\""
export PGADMIN_CONFIG_DATABASE_NAME="\"$PGADMIN_CONFIG_DATABASE_NAME\""

/entrypoint.sh "$@"
