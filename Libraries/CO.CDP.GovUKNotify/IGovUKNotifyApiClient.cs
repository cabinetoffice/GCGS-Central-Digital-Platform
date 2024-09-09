using GovukNotify.Models;

namespace CO.CDP.GovUKNotify;

public interface IGovUKNotifyApiClient
{
    Task<EmailNotificationResponse?> SendEmail(EmailNotificationResquest request);
}