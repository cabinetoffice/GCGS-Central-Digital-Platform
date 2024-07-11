using CO.CDP.EntityVerification.SQS;

namespace CO.CDP.EntityVerification.Services;

public interface IRequestListener
{
    void Register(IQueueProcessor messageReceiver);
}
