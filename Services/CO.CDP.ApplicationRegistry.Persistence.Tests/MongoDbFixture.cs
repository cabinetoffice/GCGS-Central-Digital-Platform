using CO.CDP.ApplicationRegistry.Persistence.MongoDB;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using Moq;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace CO.CDP.ApplicationRegistry.Persistence.Tests;

/// <summary>
/// Shared MongoDB container fixture.  One container is started per test collection;
/// each test class uses a unique database name so collections never collide.
/// </summary>
public class MongoDbFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder()
        .WithImage("mongo:7.0")
        .Build();

    public MongoClient Client { get; private set; } = null!;

    /// <summary>
    /// Returns a fresh <see cref="MongoAppRegistryDatabase"/> scoped to an isolated database.
    /// Indexes are created before the database is returned so index tests work out of the box.
    /// </summary>
    public MongoAppRegistryDatabase CreateDatabase(string databaseName)
    {
        var db  = Client.GetDatabase(databaseName);
        var appDb = new MongoAppRegistryDatabase(db);
        appDb.EnsureIndexes();
        return appDb;
    }

    /// <summary>Creates a stub <see cref="ICurrentUserContext"/> returning <c>"test-user"</c>.</summary>
    public ICurrentUserContext StubCurrentUser()
    {
        var mock = new Mock<ICurrentUserContext>();
        mock.Setup(u => u.UserId).Returns("test-user");
        return mock.Object;
    }

    /// <summary>Creates a no-op <see cref="IAuditRepository"/> stub.</summary>
    public IAuditRepository StubAudit()
    {
        var mock = new Mock<IAuditRepository>();
        mock.Setup(a => a.LogAsync(It.IsAny<Entities.AuditLog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    /// <summary>
    /// True when the Docker daemon was reachable and the container was started successfully.
    /// Tests should call <see cref="SkipIfUnavailable"/> at the top of each fixture's constructor.
    /// </summary>
    public bool IsAvailable { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            await _container.StartAsync();
            Client = new MongoClient(_container.GetConnectionString());
            BsonConfiguration.Register();
            IsAvailable = true;
        }
        catch (Exception ex) when (
            ex is ArgumentException ||
            ex.Message.Contains("Docker") ||
            ex.Message.Contains("docker"))
        {
            // Docker is not running. Rethrow as SkipException so xUnit marks every
            // test in this collection as Skipped rather than Failed.
            // Start Docker Desktop and re-run to exercise these tests.
            IsAvailable = false;
            throw new InvalidOperationException(
                "Docker is not running — MongoDB integration tests skipped. Start Docker Desktop and re-run.", ex);
        }
    }

    /// <summary>
    /// Call from the start of every test method that requires a real MongoDB container.
    /// Throws <see cref="SkipException"/> when Docker was unavailable at startup.
    /// </summary>
    public void SkipIfUnavailable()
    {
        if (!IsAvailable)
            throw new InvalidOperationException(
                "Docker is not running — MongoDB integration tests skipped. Start Docker Desktop and re-run.");
    }

    public async Task DisposeAsync()
    {
        if (IsAvailable)
            await _container.DisposeAsync();
    }
}

/// <summary>xUnit collection marker — all tests in this collection share one container.</summary>
[CollectionDefinition("MongoDB")]
public class MongoDbCollection : ICollectionFixture<MongoDbFixture> { }
