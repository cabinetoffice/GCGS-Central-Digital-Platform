#!/bin/bash

# Create an SQS queue
echo "Creating SQS queues..."

awslocal sqs create-queue --queue-name ev-inbound --region eu-west-1 
awslocal sqs create-queue --queue-name ev-outbound --region eu-west-1 
