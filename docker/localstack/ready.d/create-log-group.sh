#!/bin/bash

echo "Creating Log Groups..."

awslocal logs create-log-group --log-group-name ${LOG_GROUP_NAME}
awslocal logs create-log-stream --log-group-name ${LOG_GROUP_NAME} --log-stream-name ${LOG_STREAM_NAME}
