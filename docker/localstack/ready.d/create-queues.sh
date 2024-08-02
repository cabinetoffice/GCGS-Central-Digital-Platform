#!/bin/bash

echo "Creating SQS queues..."

for queue in ${SQS_QUEUES//,/ }
do
    deadletter_queue=${queue/.fifo/}-deadletter.fifo

    echo "Creating the $queue queue with $deadletter_queue DLQ..."

    deadletter_queue_url=$(awslocal sqs create-queue --queue-name "${deadletter_queue}" \
        --attributes '{"FifoQueue": "true"}' \
        --output text --query QueueUrl)
    deadletter_queue_arn=$(awslocal sqs get-queue-attributes --queue-url "$deadletter_queue_url" --attribute-names QueueArn --output text --query Attributes.QueueArn)

    queue_url=$(awslocal sqs create-queue --queue-name "${queue}" \
        --attributes '{"FifoQueue": "true", "RedrivePolicy": "{\"deadLetterTargetArn\":\"'$deadletter_queue_arn'\",\"maxReceiveCount\":\"10\"}"}' \
        --output text --query QueueUrl)
    queue_arn=$(awslocal sqs get-queue-attributes --queue-url "$queue_url" --attribute-names QueueArn --output text --query Attributes.QueueArn)

    echo "Created the $queue queue"
    echo "{"
    echo "  \"QueueUrl\": \"$queue_url\"",
    echo "  \"QueueArn\": \"$queue_arn\"",
    echo "  \"DeadLetterQueueUrl\": \"$deadletter_queue_url\""
    echo "  \"DeadLetterQueueArn\": \"$deadletter_queue_arn\""
    echo "}"

    awslocal sqs get-queue-attributes --queue-url "$queue_url" --attribute-names All
    awslocal sqs get-queue-attributes --queue-url "$deadletter_queue_url" --attribute-names All
done
