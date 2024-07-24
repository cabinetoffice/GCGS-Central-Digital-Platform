#!/bin/bash

queue_list=$(awslocal sqs list-queues --region "${REGION}")
for queue in ${QUEUES//,/ }
do
    echo "$queue_list" | grep "${queue}" || exit 1
done

bucket_list=$(awslocal s3api list-buckets)
for bucket in ${S3_BUCKETS//,/ }
do
    echo "$bucket_list" | grep "${bucket}" || exit 1
done
