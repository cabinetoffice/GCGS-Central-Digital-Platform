namespace CO.CDP.EntityVerification.Events;

public interface IEventHandler<in TEvent, out TOutcome>
{
    TOutcome Handle(TEvent @event);
}
