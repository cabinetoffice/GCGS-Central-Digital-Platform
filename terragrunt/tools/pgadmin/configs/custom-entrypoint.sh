#!/bin/sh

clear_all_servers() {
  echo "#### Removing all existing servers from pgAdmin Database ####"
  PGPASSWORD="$PGADMIN_DATABASE_PASSWORD" psql -h "$PGADMIN_DATABASE_HOST" -U "$PGADMIN_DATABASE_USERNAME" -d "$PGADMIN_DATABASE_NAME" -c "DELETE FROM public.server;"
  PGPASSWORD="$PGADMIN_DATABASE_PASSWORD" psql -h "$PGADMIN_DATABASE_HOST" -U "$PGADMIN_DATABASE_USERNAME" -d "$PGADMIN_DATABASE_NAME" -c "DELETE FROM public.servergroup;"
}

unlock_pgadmin() {
  echo "#### Unlocking all user accounts in pgAdmin ####"
  PGPASSWORD="$PGADMIN_DATABASE_PASSWORD" psql -h "$PGADMIN_DATABASE_HOST" -U "$PGADMIN_DATABASE_USERNAME" -d "$PGADMIN_DATABASE_NAME" -c "UPDATE public.user SET login_attempts = 0 WHERE login_attempts > 0;"
}

# Function to check if a server already exists in pgAdmin
check_server_exists() {
  server_name=$1
  PGPASSWORD=$PGADMIN_DATABASE_PASSWORD psql -h $PGADMIN_DATABASE_HOST -U $PGADMIN_DATABASE_USERNAME -d $PGADMIN_DATABASE_NAME -tAc "SELECT 1 FROM public.server WHERE name = '${server_name}'" | grep -q 1
  return $?
}

# Function to add a server to servers.json if it doesn't already exist
add_server_to_json() {
  local id=$1
  local name=$2
  local group=$3
  local host=$4
  local port=$5
  local maintenance_db=$6
  local username=$7

  if check_server_exists "$name"; then
    echo "$name server already exists. Skipping..."
  else
    echo "Adding $name server to servers.json..."
  echo "    \"$id\": {" >>/pgadmin4/servers.json
  echo "      \"Name\": \"$name\"," >>/pgadmin4/servers.json
  echo "      \"Group\": \"$group\"," >>/pgadmin4/servers.json
  echo "      \"Host\": \"$host\"," >>/pgadmin4/servers.json
  echo "      \"Port\": $port," >>/pgadmin4/servers.json
  echo "      \"MaintenanceDB\": \"$maintenance_db\"," >>/pgadmin4/servers.json
  echo "      \"Username\": \"$username\"," >>/pgadmin4/servers.json
  echo "      \"SSLMode\": \"prefer\"" >>/pgadmin4/servers.json
  echo "    }," >>/pgadmin4/servers.json
  fi
}

configure_servers_json() {
  echo "#### Generating servers.json: ####"

  echo '{' >/pgadmin4/servers.json
  echo '  "Servers": {' >>/pgadmin4/servers.json

  echo "#### Adding default servers ####"
  add_server_to_json "1" "admin@cdp-sirsi-pgadmin" "Admin" "$PGADMIN_DATABASE_HOST" 5432 "$PGADMIN_DATABASE_NAME" "$PGADMIN_DATABASE_USERNAME"
  add_server_to_json "4" "SIRSI Cluster" "Admin" "$DB_SIRSI_CLUSTER_ADDRESS" 5432 "$DB_SIRSI_CLUSTER_NAME" "${DB_SIRSI_CLUSTER_USERNAME}"
  add_server_to_json "5" "Entity Verification Cluster" "Admin" "$DB_ENTITY_VERIFICATION_CLUSTER_ADDRESS" 5432 "$DB_ENTITY_VERIFICATION_CLUSTER_NAME" "${DB_ENTITY_VERIFICATION_CLUSTER_USERNAME}"
  add_server_to_json "6" "SIRSI Cluster" "CDP" "$DB_SIRSI_CLUSTER_ADDRESS" 5432 "$DB_SIRSI_CLUSTER_NAME" "${DB_SIRSI_CLUSTER_USERNAME}_pgadmin"
  add_server_to_json "7" "Entity Verification Cluster" "CDP" "$DB_ENTITY_VERIFICATION_CLUSTER_ADDRESS" 5432 "$DB_ENTITY_VERIFICATION_CLUSTER_NAME" "${DB_ENTITY_VERIFICATION_CLUSTER_USERNAME}_pgadmin"

  if [ -z "$SUPPORT_USERNAMES" ]; then
    echo "#### No support users provided ####"
  else
    echo "#### Adding servers for support users ####"

    id=10
    for username in $(echo "$SUPPORT_USERNAMES" | tr ',' ' '); do
      add_server_to_json "$id" "$username@cdp-sirsi" "Production Support" "$DB_SIRSI_CLUSTER_ADDRESS" 5432 "$DB_SIRSI_CLUSTER_NAME" "$username"
      id=$((id + 1))
      add_server_to_json "$id" "$username@cdp-entity-verification" "Production Support" "$DB_ENTITY_VERIFICATION_CLUSTER_ADDRESS" 5432 "$DB_ENTITY_VERIFICATION_CLUSTER_NAME" "$username"
      id=$((id + 1))
    done
  fi

  sed -i '$ s/,$//' /pgadmin4/servers.json
  echo "  }" >>/pgadmin4/servers.json
  echo "}" >>/pgadmin4/servers.json

  cat /pgadmin4/servers.json
}

write_local_config() {
  echo "#### Updating local configurations ####"

  if [ ! -f /pgadmin4/config_local.py ]; then
    touch /pgadmin4/config_local.py
  fi
  export CONFIG_DATABASE_URI="postgresql://${PGADMIN_DATABASE_USERNAME}:${PGADMIN_DATABASE_PASSWORD}@${PGADMIN_DATABASE_HOST}:5432/${PGADMIN_DATABASE_NAME}"
  echo "import logging" >/pgadmin4/config_local.py
  echo "CONFIG_DATABASE_URI = '${CONFIG_DATABASE_URI}'" >>/pgadmin4/config_local.py
  echo "ALLOW_SAVE_PASSWORD = ${ALLOW_SAVE_PASSWORD:-False}" >>/pgadmin4/config_local.py
  echo "DEBUG = ${DEBUG:-False}" >>/pgadmin4/config_local.py
  echo "CONSOLE_LOG_LEVEL = logging.${LOG_LEVEL:-WARNING}" >>/pgadmin4/config_local.py
  echo "FILE_LOG_LEVEL = logging.${LOG_LEVEL:-WARNING}" >>/pgadmin4/config_local.py
  echo "LOGIN_BANNER = '${LOGIN_BANNER:-CDP-SIRSI PGAdmin}'" >>/pgadmin4/config_local.py
}

echo "#### Configuring pgAdmin... ####"
# clear_all_servers # Use only to force a fresh start
configure_servers_json
write_local_config
unlock_pgadmin
echo "#### Configuration of pgAdmin is complete! ####"

echo "Handing over to pgAdmin."
exec /entrypoint.sh "$@"
