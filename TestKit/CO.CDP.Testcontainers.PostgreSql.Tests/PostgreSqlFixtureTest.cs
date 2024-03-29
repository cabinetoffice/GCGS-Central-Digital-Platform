using Npgsql;

namespace CO.CDP.Testcontainers.PostgreSql.Tests;

public class PostgreSqlFixtureTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItMakesPostgreSqlAvailableInTheTest()
    {
        await using var dataSource = NpgsqlDataSource.Create(postgreSql.ConnectionString);
        await using var command = dataSource.CreateCommand("SELECT 42");
        var result = await command.ExecuteScalarAsync();
        Assert.Equal(42, result);
    }
}