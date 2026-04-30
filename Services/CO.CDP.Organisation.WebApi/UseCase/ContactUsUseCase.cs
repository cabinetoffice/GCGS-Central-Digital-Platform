using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
namespace CO.CDP.Organisation.WebApi.UseCase;

public class ContactUsUseCase(
    IGovUKNotifyApiClient govUKNotifyApiClient,
    IConfiguration configuration,
     ILogger<ContactUsUseCase> logger)
    : IUseCase<ContactUs, bool>
{
    public async Task<bool> Execute(ContactUs command)
    {
        var contactUsSentSuccess = true;

        var templateId = configuration["GOVUKNotify:ContactEmailTemplateId"] ?? "";
        var supportAdminEmailAddress = configuration["GOVUKNotify:SupportAdminEmailAddress"] ?? "";

        var missingConfigs = new List<string>();

        if (string.IsNullOrEmpty(templateId)) missingConfigs.Add("GOVUKNotify:ContactEmailTemplateId");
        if (string.IsNullOrEmpty(supportAdminEmailAddress)) missingConfigs.Add("GOVUKNotify:SupportAdminEmailAddress");

        if (missingConfigs.Count != 0)
        {
            logger.LogError(new Exception("Unable to send email "), $"Missing configuration keys: {string.Join(", ", missingConfigs)}. Unable to send email.");
            contactUsSentSuccess = false;
            return contactUsSentSuccess;
        }
       
        var emailRequest = new EmailNotificationRequest
        {
            EmailAddress = supportAdminEmailAddress,
            TemplateId = templateId,
            Personalisation = new Dictionary<string, string> {
                                        { "user_name", command.Name},
                                        { "email_address", command.EmailAddress},
                                        { "organisation_name", command.OrganisationName},
                                        { "message", command.Message},
            }
        };

        try
        {
            await govUKNotifyApiClient.SendEmail(emailRequest);
        }
        catch
        {
            contactUsSentSuccess = false;
        }

        return contactUsSentSuccess;
    }
}