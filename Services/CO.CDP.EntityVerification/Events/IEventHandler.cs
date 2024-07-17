namespace CO.CDP.EntityVerification.Events;

public interface IEventHandler
{
    void Action(IEvent msg);
}
