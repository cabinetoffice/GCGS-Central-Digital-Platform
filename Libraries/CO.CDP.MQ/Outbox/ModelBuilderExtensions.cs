using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.MQ.Outbox;

public static class ModelBuilderExtensions
{
    public static ModelBuilder OnOutboxMessageCreating(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(om =>
            {
                om.Property(m => m.CreatedOn).HasTimestampDefault();
                om.HasIndex(m => m.CreatedOn);
                om.HasIndex(m => m.Published);
            }
        );
        return modelBuilder;
    }
}