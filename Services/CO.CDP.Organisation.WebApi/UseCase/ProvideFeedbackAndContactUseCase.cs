using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
namespace CO.CDP.Organisation.WebApi.UseCase;

public class ProvideFeedbackAndContactUseCase(
    IGovUKNotifyApiClient govUKNotifyApiClient,
    IConfiguration configuration,
     ILogger<ProvideFeedbackAndContactUseCase> logger)
    : IUseCase<ProvideFeedbackAndContact, bool>
{
    public async Task<bool> Execute(ProvideFeedbackAndContact command)
    {
        var feedbackSentSuccess = true;

        var templateId = configuration["GOVUKNotify:ProvideFeedbackAndContactEmailTemplateId"] ?? "";
        var supportAdminEmailAddress = configuration["GOVUKNotify:SupportAdminEmailAddress"] ?? "";

        var missingConfigs = new List<string>();

        if (string.IsNullOrEmpty(templateId)) missingConfigs.Add("GOVUKNotify:ProvideFeedbackAndContactEmailTemplateId");
        if (string.IsNullOrEmpty(supportAdminEmailAddress)) missingConfigs.Add("GOVUKNotify:SupportAdminEmailAddress");

        if (missingConfigs.Count != 0)
        {
            logger.LogError(new Exception("Unable to send email "), $"Missing configuration keys: {string.Join(", ", missingConfigs)}. Unable to send email.");
            feedbackSentSuccess = false;
            return feedbackSentSuccess;
        }
       
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