using CO.CDP.Testcontainers.PostgreSql;
using Npgsql;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class OrganisationInformationPostgreSqlFixture : PostgreSqlFixture, IAsyncLifetime
{
    private NpgsqlDataSource? npgsqlDataSource = null;

    public NpgsqlDataSource DataSource
    {
        get
        {
            npgsqlDataSource = npgsqlDataSource ?? new NpgsqlDataSourceBuilder(ConnectionString).EnableDynamicJson().MapEnums().Build();

            return npgsqlDataSource;
        }
    }
}