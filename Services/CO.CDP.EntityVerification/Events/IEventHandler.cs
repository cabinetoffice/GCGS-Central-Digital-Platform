namespace CO.CDP.EntityVerification.Events;

public interface IEventHandler<in TEvent>
{
    Task Handle(TEvent @event);
}
