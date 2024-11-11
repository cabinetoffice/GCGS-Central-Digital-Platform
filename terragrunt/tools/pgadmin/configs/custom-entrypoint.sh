#!/bin/sh

echo "Configuring pgAdmin..."

export CONFIG_DATABASE_URI="postgresql://${PGADMIN_DATABASE_USERNAME}:${PGADMIN_DATABASE_PASSWORD}@${PGADMIN_DATABASE_HOST}:5432/${PGADMIN_DATABASE_NAME}"

clear_all_servers() {
    echo "Removing all existing servers from pgAdmin configuration..."
    PGPASSWORD=$PGADMIN_DATABASE_PASSWORD psql -h $PGADMIN_DATABASE_HOST -U $PGADMIN_DATABASE_USERNAME -d $PGADMIN_DATABASE_NAME -c "DELETE FROM public.server;"
    PGPASSWORD=$PGADMIN_DATABASE_PASSWORD psql -h $PGADMIN_DATABASE_HOST -U $PGADMIN_DATABASE_USERNAME -d $PGADMIN_DATABASE_NAME -c "DELETE FROM public.servergroup;"
}

clear_all_servers

echo "Adding all servers..."

cat <<EOF > /pgadmin4/servers.json
{
  "Servers": {
    "1": {
      "Name": "PGAdmin",
      "Group": "Admin",
      "Host": "${PGADMIN_DATABASE_HOST}",
      "Port": 5432,
      "MaintenanceDB": "${PGADMIN_DATABASE_NAME}",
      "Username": "${PGADMIN_DATABASE_USERNAME}",
      "SSLMode": "prefer"
    },
    "2": {
      "Name": "SIRSI",
      "Group": "CDP",
      "Host": "${DB_SIRSI_ADDRESS}",
      "Port": 5432,
      "MaintenanceDB": "${DB_SIRSI_NAME}",
      "Username": "${DB_SIRSI_USERNAME}",
      "SSLMode": "prefer"
    },
    "3": {
      "Name": "Entity Verification",
      "Group": "CDP",
      "Host": "${DB_ENTITY_VERIFICATION_ADDRESS}",
      "Port": 5432,
      "MaintenanceDB": "${DB_ENTITY_VERIFICATION_NAME}",
      "Username": "${DB_ENTITY_VERIFICATION_USERNAME}",
      "SSLMode": "prefer"
    },
    "4": {
      "Name": "dharmendra.verma@cdp-sirsi",
      "Group": "Production Support",
      "Host": "${DB_SIRSI_ADDRESS}",
      "Port": 5432,
      "MaintenanceDB": "${DB_SIRSI_NAME}",
      "Username": "dharmendra.verma",
      "SSLMode": "prefer"
    },
    "5": {
        "Name": "dharmendra.verma@cdp-entity-verification",
        "Group": "Production Support",
        "Host": "${DB_ENTITY_VERIFICATION_ADDRESS}",
        "Port": 5432,
        "MaintenanceDB": "${DB_ENTITY_VERIFICATION_NAME}",
        "Username": "dharmendra.verma",
        "SSLMode": "prefer"
    },
    "6": {
      "Name": "jakub.zalas@cdp-sirsi",
      "Group": "Production Support",
      "Host": "${DB_SIRSI_ADDRESS}",
      "Port": 5432,
      "MaintenanceDB": "${DB_SIRSI_NAME}",
      "Username": "jakub.zalas",
      "SSLMode": "prefer"
    },
    "7": {
        "Name": "jakub.zalas@cdp-entity-verification",
        "Group": "Production Support",
        "Host": "${DB_ENTITY_VERIFICATION_ADDRESS}",
        "Port": 5432,
        "MaintenanceDB": "${DB_ENTITY_VERIFICATION_NAME}",
        "Username": "jakub.zalas",
        "SSLMode": "prefer"
    }
  }
}
EOF

echo /pgadmin4/servers.json

echo "Baking CONFIG_DATABASE_URI in config_local.py:"
if [ ! -f /pgadmin4/config_local.py ]; then
    touch /pgadmin4/config_local.py
fi

echo "CONFIG_DATABASE_URI = '${CONFIG_DATABASE_URI}'" > /pgadmin4/config_local.py
echo "ALLOW_SAVE_PASSWORD = ${ALLOW_SAVE_PASSWORD:-False}" >> /pgadmin4/config_local.py

echo "Configuration of pgAdmin is complete!"
echo "Handing over to pgAdmin."

exec /entrypoint.sh "$@"
