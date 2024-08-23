using Microsoft.EntityFrameworkCore;

namespace CO.CDP.MQ.Database;

public interface IOutboxMessageDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; set; }
}