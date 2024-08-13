#!/bin/bash

# Function to display help
display_help() {
    echo "Usage: $0 USERNAME"
    echo
    echo "This script creates a user in AWS Cognito with a randomly generated password,"
    echo "and stores the username and password in AWS Secrets Manager."
    echo
    echo "Arguments:"
    echo "  USERNAME   The username for the new Cognito user."
    exit 1
}

generate_password() {
    local password

    # Generate a password with at least one lowercase, one uppercase, one number, and one special character
    password=$(LC_ALL=C tr -dc 'A-Za-z0-9@#%^&*()_+|~={}[]:;<>,.' < /dev/urandom | head -c 8)
    password+="$(LC_ALL=C tr -dc 'a-z' < /dev/urandom | head -c 1)"
    password+="$(LC_ALL=C tr -dc 'A-Z' < /dev/urandom | head -c 1)"
    password+="$(LC_ALL=C tr -dc '0-9' < /dev/urandom | head -c 1)"
    password+="$(LC_ALL=C tr -dc '@#%^&*()_+|~={}[]:;<>,.' < /dev/urandom | head -c 1)"

    # Shuffle the password to mix special characters with alphanumeric characters
    password=$(echo "$password" | fold -w1 | shuf | tr -d '\n')

    echo "$password"
}

if [ -z "$1" ]; then
    echo "Error: USERNAME argument is missing."
    display_help
fi

USERNAME=$1

USER_POOL_ID=$(aws cognito-idp list-user-pools --max-results 1 --query 'UserPools[0].Id' --output text)

if [ -z "$USER_POOL_ID" ]; then
    echo "Error: Unable to fetch Cognito User Pool ID."
    exit 1
fi

SECRET_NAME="cdp-sirsi-cognito/users/${USERNAME}"

RANDOM_PASSWORD=$(generate_password)

echo "Generated Password: $RANDOM_PASSWORD"

aws cognito-idp admin-create-user \
    --user-pool-id $USER_POOL_ID \
    --username $USERNAME \
    --temporary-password "$RANDOM_PASSWORD" \
    --message-action SUPPRESS | jq .

if [ $? -ne 0 ]; then
    echo "Error: Failed to create user in Cognito."
    exit 1
fi

aws cognito-idp admin-set-user-password \
    --user-pool-id $USER_POOL_ID \
    --username $USERNAME \
    --password "$RANDOM_PASSWORD" \
    --permanent | jq .

if [ $? -ne 0 ]; then
    echo "Error: Failed to mark password as permanent"
    exit 1
fi

aws secretsmanager create-secret \
    --name $SECRET_NAME \
    --secret-string "{\"username\":\"$USERNAME\",\"password\":\"$RANDOM_PASSWORD\"}" | jq .

if [ $? -ne 0 ]; then
    echo "Error: Failed to store credentials in AWS Secrets Manager."
    exit 1
fi

echo "User $USERNAME created and credentials stored in AWS Secrets Manager."
