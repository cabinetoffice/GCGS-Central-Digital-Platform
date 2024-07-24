using DotNet.Testcontainers.Images;
using Testcontainers.LocalStack;

namespace CO.CDP.AwsServices.Tests;

public class LocalStackFixture : IAsyncLifetime
{
    private readonly LocalStackContainer _localStack = new LocalStackBuilder()
        .WithImage(new DockerImage("localstack/localstack:3.5"))
        .Build();

    public string ConnectionString => _localStack.GetConnectionString();

    public Task InitializeAsync()
    {
        return _localStack.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _localStack.DisposeAsync().AsTask();
    }
}