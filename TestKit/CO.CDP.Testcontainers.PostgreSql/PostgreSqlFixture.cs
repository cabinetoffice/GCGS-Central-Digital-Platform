using Testcontainers.PostgreSql;
using Xunit;

namespace CO.CDP.Testcontainers.PostgreSql;

public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSql = new PostgreSqlBuilder().Build();

    public string ConnectionString => _postgreSql.GetConnectionString();

    public Task InitializeAsync()
    {
        return _postgreSql.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _postgreSql.DisposeAsync().AsTask();
    }
}