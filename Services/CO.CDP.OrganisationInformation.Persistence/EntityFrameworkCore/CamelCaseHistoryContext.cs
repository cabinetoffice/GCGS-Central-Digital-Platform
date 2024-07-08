using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace CO.CDP.OrganisationInformation.Persistence.EntityFrameworkCore;

/// <a href="https://github.com/efcore/EFCore.NamingConventions/issues/1">Keep history columns CamelCased</a>
// Remove this class at the time we squash migrations.
// ReSharper disable once ClassNeverInstantiated.Global
#pragma warning disable EF1001
internal class CamelCaseHistoryContext(HistoryRepositoryDependencies dependencies) : NpgsqlHistoryRepository(dependencies)
#pragma warning restore EF1001
{
    protected override void ConfigureTable(EntityTypeBuilder<HistoryRow> history)
    {
        base.ConfigureTable(history);

        // Ensure that previously created migrations table continues to work.
        // Otherwise, EF will try to access these columns by their snake_names.
        history.Property(h => h.MigrationId).HasColumnName("MigrationId");
        history.Property(h => h.ProductVersion).HasColumnName("ProductVersion");
    }
}