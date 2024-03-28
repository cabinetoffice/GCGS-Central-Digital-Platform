# CO.CDP.Tenant.Persistence

## Migrations

To generate new migration file run the following command:

```bash
dotnet ef migrations add -p Services/CO.CDP.Tenant.Persistence -s Services/CO.CDP.Tenant.WebApi <Title>
```

To generate the SQL script for the migration, run the following command:

```bash
dotnet ef migrations script -p Services/CO.CDP.Tenant.Persistence -s Services/CO.CDP.Tenant.WebApi --idempotent -o Services/CO.CDP.Tenant.Persistence/Migrations/SQL/$(date +%Y-%m-%d_%H%M)_<Title>.sql <Title>
```

Remember to replace the `<Title>` with the migration title of the latest generated SQL script.

To apply the SQL script to your local database run:

```bash
docker compose exec -iT db psql -U cdp_user -f - cdp < Services/CO.CDP.Tenant.Persistence/Migrations/SQL/*
```
