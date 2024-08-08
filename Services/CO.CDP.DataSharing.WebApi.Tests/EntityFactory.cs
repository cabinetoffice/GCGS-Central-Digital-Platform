using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace DataSharing.Tests;
internal static class EntityFactory
{
    internal static (ShareRequest, SharedConsent) GenerateSharedConsent(int organisationId, Guid organisationGuid, Guid formId)
    {
        var foundOrganisation = new Organisation
        {
            Id = organisationId,
            Guid = organisationGuid,
            Name = string.Empty,
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = string.Empty
            }
        };
;
        var formVersionId = string.Empty;

        var shareRequest = new ShareRequest
        {
            FormId = formId,
            OrganisationId = organisationGuid
        };

        var sharedConsent = new SharedConsent()
        {
            Guid = formId,
            Organisation = foundOrganisation,
            Form = new Form
            {
                Guid = formId,
                Name = string.Empty,
                Version = string.Empty,
                IsRequired = default,
                Type = default,
                Scope = default,
                Sections = new List<FormSection> { }
            },
            AnswerSets = new List<FormAnswerSet> { },
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTime.UtcNow,
            FormVersionId = formVersionId,
            BookingReference = string.Empty
        };

        return (shareRequest, sharedConsent);
    }
}
