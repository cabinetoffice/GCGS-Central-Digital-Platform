using System.Text.Json;
using CO.CDP.MQ.Database;
using Microsoft.Extensions.Logging;

namespace CO.CDP.MQ.Tests.Database;

public class DatabasePublisherContractTest : PublisherContractTest
{
    private readonly FakeOutboxMessageRepository _outboxMessageRepository = new();

    protected override async Task<T> waitForOneMessage<T>()
    {
        var message = await Task.Run(() => _outboxMessageRepository.Messages.First());
        return Deserialize<T>(message) ??
               throw new Exception($"Failed to deserialize the message: {message}.");
    }

    protected override Task<IPublisher> CreatePublisher()
    {
        return Task.FromResult<IPublisher>(new DatabasePublisher(
            _outboxMessageRepository,
            LoggerFactory.Create(_ => { }).CreateLogger<DatabasePublisher>()
        ));
    }

    private static T? Deserialize<T>(OutboxMessage message) where T : class
    {
        switch (message.Type)
        {
            case "TestMessage":
                return JsonSerializer.Deserialize<TestMessage>(message.Message) as T;
            default:
                throw new NotImplementedException($"Message type not implemented: `{message.Type}`.");
        }
    }
}

class FakeOutboxMessageRepository : IOutboxMessageRepository
{
    public List<OutboxMessage> Messages { get; set; } = new();

    public Task SaveAsync(OutboxMessage message)
    {
        Messages.Add(message);
        return Task.CompletedTask;
    }

    public Task<List<OutboxMessage>> FindOldest(int count = 10)
    {
        throw new NotImplementedException();
    }
}