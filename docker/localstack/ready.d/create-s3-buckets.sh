#!/bin/bash

echo "Creating S3 buckets..."

for bucket in ${S3_BUCKETS//,/ }
do
    awslocal s3api create-bucket --bucket "${bucket}"
done
