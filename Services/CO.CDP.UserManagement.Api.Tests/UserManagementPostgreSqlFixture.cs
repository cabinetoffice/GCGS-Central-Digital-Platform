using CO.CDP.Testcontainers.PostgreSql;
using CO.CDP.UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CO.CDP.UserManagement.Api.Tests;

public class UserManagementPostgreSqlFixture : PostgreSqlFixture
{
    private DbContextOptions<UserManagementDbContext>? _contextOptions;
    private NpgsqlDataSource? _npgsqlDataSource;

    private NpgsqlDataSource DataSource =>
        _npgsqlDataSource ??= new NpgsqlDataSourceBuilder(ConnectionString).Build();

    private DbContextOptions<UserManagementDbContext> ContextOptions =>
        _contextOptions ??= new DbContextOptionsBuilder<UserManagementDbContext>()
            .UseNpgsql(DataSource)
            .Options;

    public UserManagementDbContext UserManagementContext()
    {
        var context = new UserManagementDbContext(ContextOptions);
        context.Database.Migrate();
        context.SaveChanges();
        return context;
    }
}
