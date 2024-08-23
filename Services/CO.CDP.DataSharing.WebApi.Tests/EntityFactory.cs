using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.DataSharing.WebApi.Tests;

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

    internal static OrganisationInformation.Persistence.Forms.SharedConsent GetSharedConsent(int organisationId, Guid organisationGuid, Guid formId)
    {
        var form = new CO.CDP.OrganisationInformation.Persistence.Forms.Form
        {
            Guid = formId,
            Name = string.Empty,
            Version = string.Empty,
            IsRequired = default,
            Scope = default,
            Sections = new List<FormSection> { }
        };

        return new OrganisationInformation.Persistence.Forms.SharedConsent()
        {
            Guid = formId,
            OrganisationId = organisationId,
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
            FormId = form.Id,
            Form = form,
            AnswerSets = new List<FormAnswerSet> { },
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTime.UtcNow,
            FormVersionId = string.Empty,
            BookingReference = string.Empty
        };
    }
}
