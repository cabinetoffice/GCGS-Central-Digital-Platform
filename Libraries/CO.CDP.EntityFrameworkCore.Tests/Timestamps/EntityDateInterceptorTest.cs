using CO.CDP.EntityFrameworkCore.Timestamps;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.EntityFrameworkCore.Tests.Timestamps;

public class EntityDateInterceptorTest
{
    [Fact]
    public void ItSetsTimestampPropertiesWhenTheEntityIsFirstCreated()
    {
        var now = DateTime.Parse("2024-07-22T12:00:44.333");
        var context = DbContext(() => now);

        context.Update(new User { Name = "Bob" });
        context.SaveChanges();

        var foundUser = context.Users.FirstOrDefault(s => s.Name == "Bob");
        foundUser.Should().NotBeNull();
        foundUser.As<User>().UpdatedOn.Should().Be(DateTime.Parse("2024-07-22T12:00:44.333"));
        foundUser.As<User>().CreatedOn.Should().Be(DateTime.Parse("2024-07-22T12:00:44.333"));
    }

    [Fact]
    public void ItOnlyUpdatesTheUpdatedOnProperty()
    {
        var now = DateTime.Parse("2024-07-22T12:00:44.333");
        var context = DbContext(() => now);

        var user = new User { Name = "Bob" };
        context.Update(user);
        context.SaveChanges();

        now = DateTime.Parse("2024-08-22T13:00:00.999");
        user.Name = "Bob Updated";
        context.Update(user);
        context.SaveChanges();

        var foundUser = context.Users.FirstOrDefault(s => s.Name == "Bob Updated");
        foundUser.Should().NotBeNull();
        foundUser.As<User>().CreatedOn.Should().Be(DateTime.Parse("2024-07-22T12:00:44.333"));
        foundUser.As<User>().UpdatedOn.Should().Be(DateTime.Parse("2024-08-22T13:00:00.999"));
    }

    [Fact]
    public void ItDoesNotUpdateTheUpdatedOnPropertyIfTheEntityWasNotChanged()
    {
        var now = DateTime.Parse("2024-07-22T12:00:44.333");
        var context = DbContext(() => now);

        var user = new User { Name = "Bob" };
        context.Update(user);
        context.SaveChanges();

        now = DateTime.Parse("2024-08-22T13:00:00.999");
        context.SaveChanges();

        var foundUser = context.Users.FirstOrDefault(s => s.Name == "Bob");
        foundUser.Should().NotBeNull();
        foundUser.As<User>().CreatedOn.Should().Be(DateTime.Parse("2024-07-22T12:00:44.333"));
        foundUser.As<User>().UpdatedOn.Should().Be(DateTime.Parse("2024-07-22T12:00:44.333"));
    }

    [Fact]
    public async Task ItSetsTimestampPropertiesInAsyncCall()
    {
        var now = DateTime.Parse("2024-07-22T12:00:44.333");
        var context = DbContext(() => now);

        context.Update(new User { Name = "Bob" });
        await context.SaveChangesAsync();

        var foundUser = context.Users.FirstOrDefault(s => s.Name == "Bob");
        foundUser.Should().NotBeNull();
        foundUser.As<User>().UpdatedOn.Should().Be(DateTime.Parse("2024-07-22T12:00:44.333"));
        foundUser.As<User>().CreatedOn.Should().Be(DateTime.Parse("2024-07-22T12:00:44.333"));
    }

    private static int _counter;
    private TestDbContext DbContext(Func<DateTime> clock)
    {
        return new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: $"test-db-{Interlocked.Increment(ref _counter)}")
            .AddInterceptors(new EntityDateInterceptor(clock))
            .Options);
    }
}

internal class TestDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}

internal class User : IEntityDate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}