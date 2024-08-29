using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.MQ.Database;

public static class ModelBuilderExtensions
{
    public static ModelBuilder OnOutboxMessageCreating(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(om =>
            om.Property(m => m.CreatedOn).HasTimestampDefault()
        );
        return modelBuilder;
    }
}