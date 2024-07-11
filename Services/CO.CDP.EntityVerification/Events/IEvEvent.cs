using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Services;

namespace CO.CDP.EntityVerification.Events;

public interface IEvEvent
{
    void Action(EvMessage msg);
}
