using CO.CDP.GovUKNotify.Models;

namespace CO.CDP.GovUKNotify;

public interface IGovUKNotifyApiClient
{
    Task<EmailNotificationResponse?> SendEmail(EmailNotificationRequest request);
}