#!/bin/bash

echo "Creating SQS queues..."

for queue in ${QUEUES//,/ }
do
    awslocal sqs create-queue --queue-name "${queue}"
done
