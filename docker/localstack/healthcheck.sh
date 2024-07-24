#!/bin/bash

awslocal sqs list-queues --region "${REGION}" | grep -E "${QUEUES//,/|}" || exit 1
