using CO.CDP.MQ.Database;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.MQ.Tests.Database;

internal class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options), IOutboxMessageDbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
}