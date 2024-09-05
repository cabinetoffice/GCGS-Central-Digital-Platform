using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using FormSection = CO.CDP.OrganisationInformation.Persistence.Forms.FormSection;

namespace CO.CDP.Forms.WebApi.Tests;

internal static class EntityFactory
{
    internal static SharedConsent GetSharedConsent(int organisationId, Guid organisationGuid, Guid formId)
    {
        var form = new Form
        {
            Guid = formId,
            Name = string.Empty,
            Version = string.Empty,
            IsRequired = default,
            Scope = default,
            Sections = new List<FormSection> { }
        };

        var orgnisation = new Organisation
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

        return new SharedConsent()
        {
            Guid = formId,
            OrganisationId = orgnisation.Id,
            Organisation = orgnisation,
            FormId = form.Id,
            Form = form,
            AnswerSets = new List<FormAnswerSet> { },
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTime.UtcNow,
            FormVersionId = string.Empty,
            ShareCode = string.Empty

        };
    }

    internal static SharedConsent GetSharedConsentWithMixedTypeDeclarationAnswerSets(int organisationId, Guid organisationGuid, Guid formId)
    {
        var form = new Form
        {
            Guid = formId,
            Name = string.Empty,
            Version = string.Empty,
            IsRequired = default,
            Scope = default,
            Sections = new List<FormSection> { }
        };

        var orgnisation = new Organisation
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

        return new SharedConsent()
        {
            Guid = formId,
            OrganisationId = orgnisation.Id,
            Organisation = orgnisation,
            FormId = form.Id,
            Form = form,
            AnswerSets = new List<FormAnswerSet>
            {
                new FormAnswerSet
                {
                    Guid = Guid.NewGuid(),
                    SharedConsentId = default,
                    SharedConsent = null,
                    SectionId = default,
                    Section = new FormSection
                    {
                        Guid = Guid.NewGuid(),
                        Title = string.Empty,
                        Type = FormSectionType.Standard,
                        FormId = form.Id,
                        Form = null,
                        Questions = null,
                        AllowsMultipleAnswerSets = default,
                        Configuration = null
                    },
                    Answers = new List<FormAnswer>() { }
                },
                new FormAnswerSet
                {
                    Guid = Guid.NewGuid(),
                    SharedConsentId = default,
                    SharedConsent = null,
                    SectionId = default,
                    Section = new FormSection
                    {
                        Guid = Guid.NewGuid(),
                        Title = string.Empty,
                        Type = FormSectionType.Declaration,
                        FormId = form.Id,
                        Form = null,
                        Questions = null,
                        AllowsMultipleAnswerSets = default,
                        Configuration = null
                    },
                    Answers = new List<FormAnswer>() { }
                }
            },
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTime.UtcNow,
            FormVersionId = string.Empty,
            ShareCode = string.Empty
        };
    }

    internal static SharedConsent GetSharedConsentWithDeclarationOnlyAnswerSets(int organisationId, Guid organisationGuid, Guid formId)
    {
        var form = new Form
        {
            Guid = formId,
            Name = string.Empty,
            Version = string.Empty,
            IsRequired = default,
            Scope = default,
            Sections = new List<FormSection> { }
        };

        var orgnisation = new Organisation
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

        return new SharedConsent()
        {
            Guid = formId,
            OrganisationId = orgnisation.Id,
            Organisation = orgnisation,
            FormId = form.Id,
            Form = form,
            AnswerSets = new List<FormAnswerSet>
            {
                new FormAnswerSet
                {
                    Guid = Guid.NewGuid(),
                    SharedConsentId = default,
                    SharedConsent = null,
                    SectionId = default,
                    Section = new FormSection
                    {
                        Guid = Guid.NewGuid(),
                        Title = string.Empty,
                        Type = FormSectionType.Declaration,
                        FormId = form.Id,
                        Form = null,
                        Questions = null,
                        AllowsMultipleAnswerSets = default,
                        Configuration = null
                    },
                    Answers = new List<FormAnswer>() { }
                },
                new FormAnswerSet
                {
                    Guid = Guid.NewGuid(),
                    SharedConsentId = default,
                    SharedConsent = null,
                    SectionId = default,
                    Section = new FormSection
                    {
                        Guid = Guid.NewGuid(),
                        Title = string.Empty,
                        Type = FormSectionType.Declaration,
                        FormId = form.Id,
                        Form = null,
                        Questions = null,
                        AllowsMultipleAnswerSets = default,
                        Configuration = null
                    },
                    Answers = new List<FormAnswer>() { }
                }
            },
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTime.UtcNow,
            FormVersionId = string.Empty,
            ShareCode = string.Empty

        };
    }
}