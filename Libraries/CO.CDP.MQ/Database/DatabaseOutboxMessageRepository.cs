using Microsoft.EntityFrameworkCore;

namespace CO.CDP.MQ.Database;

public class DatabaseOutboxMessageRepository<TC>(TC context) : IOutboxMessageRepository
    where TC : DbContext, IOutboxMessageDbContext
{
    public async Task SaveAsync(OutboxMessage message)
    {
        context.Update(message);
        await context.SaveChangesAsync();
    }

    public async Task<List<OutboxMessage>> FindOldest(int count = 10)
    {
        return await context.OutboxMessages
            .OrderBy(o => o.CreatedOn)
            .Take(count)
            .ToListAsync();
    }
}