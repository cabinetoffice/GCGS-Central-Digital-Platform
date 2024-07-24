#!/bin/bash

echo "Creating SQS queues..."

for queue in ${QUEUES//,/ }
do
    awslocal sqs create-queue --queue-name "${queue}" --region "${DEFAULT_REGION:-eu-west-1}"
done
