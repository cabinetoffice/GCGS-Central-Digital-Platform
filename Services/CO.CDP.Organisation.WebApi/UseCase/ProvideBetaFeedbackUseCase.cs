using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
namespace CO.CDP.Organisation.WebApi.UseCase;

public class ProvideBetaFeedbackUseCase(
    IGovUKNotifyApiClient govUKNotifyApiClient,
    IConfiguration configuration)
    : IUseCase<ProvideFeedback, bool>
{
    public async Task<bool> Execute(ProvideFeedback command)
    {
        var feedbackSentSuccess = true;
        var templateId = configuration.GetValue<string>("GOVUKNotify:ProvideFeedbackEmailTemplateId") ?? "";
        var supportAdminEmailAddress = configuration.GetValue<string>("GOVUKNotify:SupportAdminEmailAddress") ?? "";
        var emailRequest = new EmailNotificationRequest
        {
            EmailAddress = supportAdminEmailAddress,
            TemplateId = templateId,
            Personalisation = new Dictionary<string, string> {
                                        { "feedback_about", command.FeedbackAbout},
                                        { "specific_page", command.SpecificPage},
                                        { "feedback", command.Feedback},
                                        { "name", command.Name},
                                        { "email", command.Email }
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