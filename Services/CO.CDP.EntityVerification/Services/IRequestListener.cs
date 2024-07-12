namespace CO.CDP.EntityVerification.Services;

public interface IRequestListener
{
    void Register(QueueBackgroundService messageReceiver);
}
