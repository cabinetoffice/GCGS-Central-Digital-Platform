using CO.CDP.Testcontainers.PostgreSql;
using CO.CDP.UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.Redis;

namespace CO.CDP.UserManagement.Api.Tests;

public class UserManagementPostgreSqlFixture : PostgreSqlFixture
{
    private readonly RedisContainer _redis = new RedisBuilder().Build();

    private DbContextOptions<UserManagementDbContext>? _contextOptions;
    private NpgsqlDataSource? _npgsqlDataSource;

    public string RedisHost => _redis.Hostname;
    public string RedisPort => _redis.GetMappedPublicPort(6379).ToString();

    private NpgsqlDataSource DataSource =>
        _npgsqlDataSource ??= new NpgsqlDataSourceBuilder(ConnectionString).Build();

    private DbContextOptions<UserManagementDbContext> ContextOptions =>
        _contextOptions ??= new DbContextOptionsBuilder<UserManagementDbContext>()
            .UseNpgsql(DataSource)
            .Options;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await _redis.StartAsync();
    }

    public override async Task DisposeAsync()
    {
        await _redis.DisposeAsync();
        await base.DisposeAsync();
    }

    public UserManagementDbContext UserManagementContext()
    {
        var context = new UserManagementDbContext(ContextOptions);
        context.Database.Migrate();
        context.SaveChanges();
        return context;
    }
}
