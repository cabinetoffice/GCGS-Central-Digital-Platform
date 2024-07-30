using CO.CDP.Forms.WebApiClient;

namespace CO.CDP.OrganisationApp.WebApiClients;

internal static class FormsClientExtensions
{
    internal static async Task SaveUpdateAnswers(
         this IFormsClient formsApiClient,
         Guid formId,
         Guid sectionId,
         Guid organisationId,
         Guid answerSetId,
         List<FormAnswer> answers)
         => await formsApiClient.PutFormSectionAnswersAsync(
             formId,
             sectionId,
             answerSetId,
             organisationId,
             new UpdateFormSectionAnswers(
                 answers: answers
             )
         );
}
