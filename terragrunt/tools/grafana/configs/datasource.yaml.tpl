apiVersion: 1

datasources:
  - name: Cloudwatch
    type: cloudwatch
    uid: CLOUDWATCH
    isDefault: false
    jsonData:
      defaultRegion: "eu-west-2"
      authType: "default"
      assumeRoleArn: "__GRAFANA_CLOUDWATCH_ROLE__"
