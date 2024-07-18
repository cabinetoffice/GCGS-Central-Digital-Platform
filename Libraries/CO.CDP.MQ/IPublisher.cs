namespace CO.CDP.MQ;

public interface IPublisher : IDisposable
{
    void Publish<TM>(TM message) where TM : notnull;
}