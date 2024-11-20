#!/bin/bash

DEFAULT_STEP_FUNCTION_ARN="arn:aws:states:eu-west-2:891377225335:stateMachine:cdp-sirsi-slack-notification-middleman"
DEFAULT_EVENTS_DIR="../../secrets/pipeline_inputs/disordered_events"

STEP_FUNCTION_ARN="${1:-$DEFAULT_STEP_FUNCTION_ARN}"
EVENTS_DIR="${2:-$DEFAULT_EVENTS_DIR}"

echo "Using Step Function ARN: $STEP_FUNCTION_ARN"
echo "Using Events Directory: $EVENTS_DIR"

for i in {01..12}; do
    JSON_FILE="${EVENTS_DIR}/${i}.json"

    if [[ -f "$JSON_FILE" ]]; then
        echo "Triggering Step Function with $JSON_FILE..."

        # Invoke the Step Function with the JSON file
        aws stepfunctions start-execution \
            --state-machine-arn "$STEP_FUNCTION_ARN" \
            --input "file://$JSON_FILE" | jq .

        sleep 1

        echo "Triggered with $JSON_FILE"
    else
        echo "File $JSON_FILE not found or does not match the expected pattern, skipping..."
    fi
done
