using CO.CDP.EntityVerification.Model;

namespace CO.CDP.EntityVerification.Events;

public interface IEventHandler
{
    void Action(EvMessage msg);
}
