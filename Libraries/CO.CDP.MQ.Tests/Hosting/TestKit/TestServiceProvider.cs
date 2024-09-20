namespace CO.CDP.MQ.Tests.Hosting.TestKit;

internal class TestServiceProvider : IServiceProvider
{
    public readonly Dictionary<Type, object> Services = new();

    public object? GetService(Type serviceType)
    {
        return Services.GetValueOrDefault(serviceType);
    }
}