## Access via OpenSearch tools (admin, gateway, debugtask)

OpenSearch tools are behind **separate Cognito user pools**:
- Admin pool: `cdp-sirsi-opensearch-admin` (domain prefix: `cdp-sirsi-opensearch-admin`)
- Gateway pool: `cdp-sirsi-opensearch-gateway` (domain prefix: `cdp-sirsi-opensearch-gateway`)
- Debugtask pool: `cdp-sirsi-opensearch-debugtask` (domain prefix: `cdp-sirsi-opensearch-debugtask`)

App clients:
- `cdp-sirsi-<env>-opensearch-admin`
- `cdp-sirsi-<env>-opensearch-gateway`
- `cdp-sirsi-<env>-opensearch-debugtask`

User management for these clients (create users, reset password, etc.) is documented in
`docs/tools-users.md`.

## Bootstrap process (new environments)

OpenSearch uses **advanced security** with an external master user (IAM role). The master user is:
- `cdp-sirsi-ecs-task-opensearch-admin`

That role must be used to bootstrap OpenSearch Security **roles and role mappings** for the app and gateway.
You can do this via the **opensearch-admin** service (recommended).

Minimum bootstrap steps (run once per environment):

1) Ensure opensearch-admin is deployed and reachable.
2) Apply the **service writer** role + mapping (application role).
3) Apply the **gateway readonly** role + mapping (gateway role).

Example bootstrap commands are included in the **Permissions and role mappings** section below.

## Dev Tools quick checks

These examples reflect the **development** account and the current index naming convention.
We standardize on:
- `fts_*` for OpenSearch indices
- `.fts_*` for internal/migration indices

### Cluster and node health

```bash
GET /
GET _cluster/health
GET _cat/nodes?v
GET _cat/indices?v
```

### Index discovery (current naming convention)

```bash
GET _cat/indices/fts_*?v
GET _cat/indices/.fts_*?v
```

### Security: confirm auth context

```bash
GET _plugins/_security/authinfo
GET /_plugins/_security/api/rolesmapping
```

## Permissions and role mappings

These commands are intended to be run **in the opensearch-admin Dev Tools console**
and in this order. Replace `<ACCOUNT_ID>` with the target account ID.

```bash
# Action group used by the service writer role (covers bulk and bulk*).
GET _plugins/_security/api/actiongroups/fts_write_with_bulk
PUT _plugins/_security/api/actiongroups/fts_write_with_bulk
{
  "allowed_actions": [
    "indices:data/write/bulk",
    "indices:data/write/bulk*",
    "indices:data/write/*",
    "indices:data/read/*",
    "indices:admin/*"
  ]
}

# Service writer role (application + debugtask)
GET _plugins/_security/api/roles/cdp_sirsi_service_writer
PUT _plugins/_security/api/roles/cdp_sirsi_service_writer
{
  "cluster_permissions": [
    "cluster:monitor/main",
    "cluster:monitor/health"
  ],
  "index_permissions": [
    {
      "index_patterns": [
        "fts_*",
        ".fts_*"
      ],
      "allowed_actions": [
        "fts_write_with_bulk"
      ]
    },
    {
      "index_patterns": [
        ".opensearch_dashboards*",
        ".kibana*",
        ".kibana_task_manager*"
      ],
      "allowed_actions": [
        "fts_write_with_bulk"
      ]
    }
  ],
  "tenant_permissions": []
}

GET /_plugins/_security/api/rolesmapping/cdp_sirsi_service_writer
PUT /_plugins/_security/api/rolesmapping/cdp_sirsi_service_writer
{
  "backend_roles": [
    "arn:aws:iam::<ACCOUNT_ID>:role/cdp-sirsi-ecs-task"
  ],
  "hosts": [],
  "users": []
}

# Dashboards role mapping (required for Dev Tools to avoid bulk 403s)
# Built-in dashboards role mapping
GET _plugins/_security/api/roles/opensearch_dashboards_user
PUT _plugins/_security/api/rolesmapping/opensearch_dashboards_user
{
  "backend_roles": [
    "arn:aws:iam::<ACCOUNT_ID>:role/cdp-sirsi-ecs-task"
  ]
}

# Gateway readonly role
GET _plugins/_security/api/roles/cdp_sirsi_gateway_readonly
PUT _plugins/_security/api/roles/cdp_sirsi_gateway_readonly
{
  "cluster_permissions": [
    "cluster:monitor/main",
    "cluster:monitor/health"
  ],
  "index_permissions": [
    {
      "index_patterns": [
        "fts_*",
        ".fts_*"
      ],
      "allowed_actions": [
        "indices:admin/aliases/get",
        "indices:admin/exists",
        "indices:data/read/search",
        "indices:data/read/get"
      ]
    }
  ],
  "tenant_permissions": []
}

GET _plugins/_security/api/rolesmapping/cdp_sirsi_gateway_readonly
PUT _plugins/_security/api/rolesmapping/cdp_sirsi_gateway_readonly
{
  "backend_roles": [
    "arn:aws:iam::<ACCOUNT_ID>:role/cdp-sirsi-ecs-task-opensearch-readonly",
    "arn:aws:iam::<ACCOUNT_ID>:role/cdp-sirsi-ecs-task-opensearch-gateway"
  ],
  "hosts": [],
  "users": []
}

# Admin mappings (if needed in dev)
GET _plugins/_security/api/rolesmapping/all_access
PUT _plugins/_security/api/rolesmapping/all_access
{
  "backend_roles": [
    "arn:aws:iam::<ACCOUNT_ID>:role/cdp-sirsi-ecs-task-opensearch-admin"
  ],
  "hosts": [],
  "users": []
}

GET _plugins/_security/api/rolesmapping/security_manager
PUT _plugins/_security/api/rolesmapping/security_manager
{
  "backend_roles": [
    "arn:aws:iam::<ACCOUNT_ID>:role/cdp-sirsi-ecs-task-opensearch-admin"
  ],
  "hosts": [],
  "users": []
}
```

## Index and alias operations (examples)

## Debugtask permission check (fts_* and .fts_*)

These commands are safe to run from **opensearch-debugtask** to validate the
`cdp_sirsi_service_writer` permissions. They use test indices so cleanup is easy.

### Create test indices

```bash
PUT fts_debug_test_public
{
  "settings": {
    "index": {
      "number_of_shards": 1,
      "number_of_replicas": 0
    }
  }
}

PUT .fts_debug_test_internal
{
  "settings": {
    "index": {
      "number_of_shards": 1,
      "number_of_replicas": 0
    }
  }
}
```

### Write and read

```bash
POST fts_debug_test_public/_doc
{ "message": "public index write ok" }

POST .fts_debug_test_internal/_doc
{ "message": "internal index write ok" }

GET fts_debug_test_public/_search
{
  "query": { "match_all": {} }
}

GET .fts_debug_test_internal/_search
{
  "query": { "match_all": {} }
}
```

### Alias operations

```bash
POST _aliases
{
  "actions": [
    { "add": { "index": "fts_debug_test_public", "alias": "fts_debug_test_current" } },
    { "add": { "index": ".fts_debug_test_internal", "alias": ".fts_debug_test_current" } }
  ]
}

GET fts_debug_test_current/_search
{
  "query": { "match_all": {} }
}

GET .fts_debug_test_current/_search
{
  "query": { "match_all": {} }
}
```

### Cleanup

```bash
DELETE fts_debug_test_public
DELETE .fts_debug_test_internal
```

## Troubleshooting (403s / permissions)

Use this block from **opensearch-admin Dev Tools** to verify roles/mappings and clear caches.

```bash
# Confirm which backend role you're authenticated as
GET _plugins/_security/authinfo

# Inspect role definitions (service writer + dashboards writer)
GET _plugins/_security/api/roles/cdp_sirsi_service_writer
GET _plugins/_security/api/roles/cdp_sirsi_dashboards_writer
GET _plugins/_security/api/roles/opensearch_dashboards_user

# Inspect role mappings (service writer + dashboards writer + dashboards user)
GET _plugins/_security/api/rolesmapping/cdp_sirsi_service_writer
GET _plugins/_security/api/rolesmapping/cdp_sirsi_dashboards_writer
GET _plugins/_security/api/rolesmapping/opensearch_dashboards_user

# Clear security cache after role/mapping updates
DELETE _plugins/_security/api/cache

# NOTE: "allow_restricted_indices" is not supported in this OpenSearch version.
# If you see errors like unrecognized_property_exception, remove that field.
```

### Create a test index

```bash
PUT fts_test_permissions
{
  "settings": {
    "index": {
      "number_of_shards": 1,
      "number_of_replicas": 0
    }
  },
  "mappings": {
    "properties": {
      "id": { "type": "keyword" },
      "title": { "type": "text" },
      "createdAt": { "type": "date" },
      "tags": { "type": "keyword" }
    }
  }
}
```

### Index data

```bash
PUT fts_test_permissions/_doc/1
{
  "id": "1",
  "title": "First test document",
  "createdAt": "2026-01-27T21:00:00Z",
  "tags": ["dev", "fts"]
}

POST _bulk
{ "index": { "_index": "fts_test_permissions", "_id": "2" } }
{ "id": "2", "title": "Second test document", "createdAt": "2026-01-27T21:05:00Z", "tags": ["dev"] }
{ "index": { "_index": "fts_test_permissions", "_id": "3" } }
{ "id": "3", "title": "Third test document", "createdAt": "2026-01-27T21:10:00Z", "tags": ["fts"] }

POST fts_test_permissions/_refresh
```

### Query data

```bash
GET fts_test_permissions/_search
{
  "query": {
    "match": {
      "title": "test"
    }
  }
}

GET fts_test_permissions/_count
{
  "query": {
    "match_all": {}
  }
}
```

### Alias operations

```bash
POST _aliases
{
  "actions": [
    { "add": { "index": "fts_test_permissions", "alias": "fts_test_current" } }
  ]
}

GET fts_test_current/_search
{
  "query": { "match_all": {} }
}
```

### Cleanup

```bash
DELETE fts_test_permissions/_doc/2
DELETE fts_test_permissions
```
