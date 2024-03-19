using CO.CDP.Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Tenant.Persistence.Tests;

public class DatabaseTenantRepositoryTest(PostgreSqlFixture postgreSql) :
    TenantRepositoryContractTest, IClassFixture<PostgreSqlFixture>
{
    protected override ITenantRepository TenantRepository()
    {
        return new DatabaseTenantRepository(TenantContext());
    }

    private TenantContext TenantContext()
    {
        var context = new TenantContext(postgreSql.ConnectionString);
        context.Database.Migrate();
        context.SaveChanges();
        return context;
    }
}