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
        }
    }
}
EOF

/entrypoint.sh "$@"
