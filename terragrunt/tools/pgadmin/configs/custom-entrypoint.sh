#!/bin/sh

echo "Configuring pgAdmin..."

cat <<EOF > /pgadmin4/servers.json
{
    "Servers": {
        "cdp_sirsi": {
            "Name": "SIRSI RDS PostgreSQL",
            "Group": "Servers",
            "Host": "${DB_ADDRESS}",
            "Port": 5432,
            "MaintenanceDB": "${DB_NAME}",
            "Username": "${DB_USERNAME}",
            "Password": "${DB_PASSWORD}",
            "SSLMode": "prefer"
        }
    }
}
EOF

/entrypoint.sh "$@"
