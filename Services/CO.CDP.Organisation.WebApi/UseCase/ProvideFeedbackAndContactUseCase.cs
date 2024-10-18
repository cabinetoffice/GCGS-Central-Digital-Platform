using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
namespace CO.CDP.Organisation.WebApi.UseCase;

public class ProvideFeedbackAndContactUseCase(
    IGovUKNotifyApiClient govUKNotifyApiClient,
    IConfiguration configuration)
    : IUseCase<ProvideFeedbackAndContact, bool>
{
    public async Task<bool> Execute(ProvideFeedbackAndContact command)
    {
        var feedbackSentSuccess = true;

        // TODO: select template based on Subject
        var templateId = configuration["GOVUKNotify:ProvideFeedbackEmailTemplateId"] ?? "";

        var supportAdminEmailAddress = configuration["GOVUKNotify:SupportAdminEmailAddress"] ?? "";
        var emailRequest = new EmailNotificationRequest
        {
            EmailAddress = supportAdminEmailAddress,
            TemplateId = templateId,
            Personalisation = new Dictionary<string, string> {
                                        { "feedback_about", command.FeedbackAbout},
                                        { "specific_page", command.SpecificPage},
                                        { "feedback", command.Feedback},
                                        { "name", command.Name},
                                        { "email", command.Email },
                                        { "subject", command.Subject }
            }
        };

        try
        {
            await govUKNotifyApiClient.SendEmail(emailRequest);
        }
        catch (Exception)
        {
            feedbackSentSuccess = false;
        }

        return feedbackSentSuccess;
    }
}