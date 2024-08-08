using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace DataSharing.Tests;
internal static class EntityFactory
{
    internal static ShareRequest GetShareRequest(Guid organisationGuid, Guid formId)
    {
        return new ShareRequest
        {
            FormId = formId,
            OrganisationId = organisationGuid
        };
    }

    internal static SharedConsent GetSharedConsent(int organisationId, Guid organisationGuid, Guid formId)
    {
        return new SharedConsent()
        {
            Guid = formId,
            Organisation = new Organisation
            {
                Id = organisationId,
                Guid = organisationGuid,
                Name = string.Empty,
                Tenant = new Tenant
                {
                    Guid = Guid.NewGuid(),
                    Name = string.Empty
                }
            },
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
            FormVersionId = string.Empty,
            BookingReference = string.Empty
        };
    }
}
