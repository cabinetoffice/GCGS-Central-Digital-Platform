using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;

namespace CO.CDP.ScheduledWorker;

public class CompleteMoUReminderService(
    ILogger<CompleteMoUReminderService> logger,
    IMouRepository mouRepository,
    IConfiguration configuration,
    IGovUKNotifyApiClient govUKNotifyClient) : IScopedProcessingService
{
    public async Task ExecuteWorkAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Complete MoU Reminder Processing Service started.");

        var reminderList = await mouRepository.GetMouReminderOrganisations(DaysBetweenEmailReminders);

        foreach (var organisation in reminderList)
        {
            await NotifyCompleteMoUReminder(organisation);

            await mouRepository.UpsertMouEmailReminder(organisation.Id);

            logger.LogInformation($"Sent MoU Reminder Email For {organisation.Name}.");
        }

        logger.LogInformation("Complete MoU Reminder Processing Service finished.");
    }

    private async Task NotifyCompleteMoUReminder(MouReminderOrganisation organisation)
    {
        var emailRecipients = organisation.Email.Split(",", StringSplitOptions.RemoveEmptyEntries);

        var emailRequest = new EmailNotificationRequest
        {
            EmailAddress = emailRecipients[0],
            TemplateId = EmailTemplateId,
            Personalisation = new Dictionary<string, string>
            {
                { "org_name", organisation.Name },
                { "link", new Uri(new Uri(OrganisationAppUrl), $"/organisation/{organisation.Guid}/review-and-sign-memorandom").ToString() }
            }
        };

        foreach (var email in emailRecipients)
        {
            try
            {
                emailRequest.EmailAddress = email;
                await govUKNotifyClient.SendEmail(emailRequest);
            }
            catch
            {
                logger.LogError($"Failed to send Mou reminder email for: {organisation.Name}");
            }
        }
    }

    private int DaysBetweenEmailReminders =>
        configuration.GetValue<int?>("CompleteMoUReminderJob:DaysBetweenEmailReminders") ??
                throw new Exception("Missing configuration keys: CompleteMoUReminderJob:DaysBetweenEmailReminders.");

    private string OrganisationAppUrl =>
        configuration.GetValue<string>("OrganisationAppUrl") ??
                throw new Exception("Missing configuration keys: OrganisationAppUrl.");

    private string EmailTemplateId =>
        configuration.GetValue<string>("GOVUKNotify:MouReminderToSignEmailTemplateId") ??
                throw new Exception("Missing configuration keys: GOVUKNotify:MouReminderToSignEmailTemplateId.");
}