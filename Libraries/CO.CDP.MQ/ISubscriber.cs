namespace CO.CDP.MQ;

public interface ISubscriber<in TEvent> where TEvent : class
{
    Task Handle(TEvent @event);
}