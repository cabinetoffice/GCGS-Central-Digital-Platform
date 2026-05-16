apiVersion: 1

contactPoints:
  - orgId: 1
    name: Microsoft Teams
    receivers:
      - uid: teams-default
        type: teams
        settings:
          url: "__TEAMS_WEBHOOK_URL__"

notificationPolicies:
  - orgId: 1
    receiver: Microsoft Teams
    group_by:
      - grafana_folder
      - alertname

muteTimeIntervals: []
