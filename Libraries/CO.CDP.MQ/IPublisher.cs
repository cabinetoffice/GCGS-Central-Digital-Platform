namespace CO.CDP.MQ;

public interface IPublisher : IDisposable
{
    Task Publish<TM>(TM message) where TM : notnull;
}