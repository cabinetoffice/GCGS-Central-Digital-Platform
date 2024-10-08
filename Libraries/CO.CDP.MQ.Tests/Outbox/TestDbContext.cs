using CO.CDP.EntityFrameworkCore.Timestamps;
using CO.CDP.MQ.Outbox;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.MQ.Tests.Outbox;

internal class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options), IOutboxMessageDbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.OnOutboxMessageCreating();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new EntityDateInterceptor());
    }
}