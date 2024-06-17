#!/bin/bash
set -e

sed -i "s/GRAFANA_CLOUDWATCH_ACCOUNT_ID/${GRAFANA_CLOUDWATCH_ACCOUNT_ID}/g" /etc/grafana/provisioning/dashboards/application/*

sed -i "s#__GRAFANA_CLOUDWATCH_ROLE__#${GRAFANA_CLOUDWATCH_ROLE}#g" /etc/grafana/provisioning/datasources/datasource.yaml.tpl

mv /etc/grafana/provisioning/datasources/datasource.yaml.tpl /etc/grafana/provisioning/datasources/datasource.yaml


/run.sh
