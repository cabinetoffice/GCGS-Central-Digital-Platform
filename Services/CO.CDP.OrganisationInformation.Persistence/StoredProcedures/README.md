# Stored procedure management

This project uses Git-managed stored procedures within migrations to ensure versioned changes to database logic.

EF Core migrations apply SQL files using `migrationBuilder.Sql(File.ReadAllText(...))`.
This allows stored procedure definitions to be kept in .psql files under version control.

## Steps

- Before you make changes, copy the "current" version of the stored procedure to the `-PREVIOUS.psql` file.
- Update the main stored procedure script with your changes
- Create a new migration based on the following skeleton

```csharp
/// <inheritdoc />
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(File.ReadAllText("Migrations/StoredProcedures/create_shared_consent_snapshot.psql"));
}

/// <inheritdoc />
protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(File.ReadAllText("Migrations/StoredProcedures/create_shared_consent_snapshot-PREVIOUS.psql"));
}
```

If creating a new stored procedure script file, make sure it is set to be copied to the output directory in the file properties.