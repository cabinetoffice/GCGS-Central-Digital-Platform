using Microsoft.EntityFrameworkCore;

namespace CO.CDP.MQ.Outbox;

public interface IOutboxMessageDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; set; }
}