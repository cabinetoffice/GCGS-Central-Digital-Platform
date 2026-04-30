using DotNet.Testcontainers.Images;
using Testcontainers.LocalStack;

namespace CO.CDP.UserManagement.Api.Tests.Integration;

/// <summary>
/// xUnit class fixture that starts a LocalStack container for integration tests that
/// interact with AWS services (e.g. SQS).
/// </summary>
public class LocalStackFixture : IAsyncLifetime
{
    private readonly LocalStackContainer _localStack = new LocalStackBuilder()
        .WithImage(new DockerImage("localstack/localstack:3.5"))
        .Build();

    /// <summary>Gets the LocalStack base URL (e.g. <c>http://localhost:4566</c>).</summary>
    public string ConnectionString => _localStack.GetConnectionString();

    public Task InitializeAsync() => _localStack.StartAsync();

    public Task DisposeAsync() => _localStack.DisposeAsync().AsTask();
}