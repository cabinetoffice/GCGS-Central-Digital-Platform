using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CO.CDP.EntityFrameworkCore.Timestamps;

public class EntityDateInterceptor(Func<DateTime> clock) : SaveChangesInterceptor
{
    public EntityDateInterceptor() : this(() => DateTime.UtcNow)
    {
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateTimestamps(eventData);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        UpdateTimestamps(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateTimestamps(DbContextEventData eventData)
    {
        var entries = eventData.Context?.ChangeTracker.Entries<IEntityDate>() ?? [];
        var now = clock();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOn = now;
                entry.Entity.UpdatedOn = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedOn = now;
            }
        }
    }
}