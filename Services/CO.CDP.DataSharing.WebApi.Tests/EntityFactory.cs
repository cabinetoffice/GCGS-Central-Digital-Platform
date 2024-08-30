using CO.CDP.DataSharing.WebApi.Extensions;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using FormSection = CO.CDP.OrganisationInformation.Persistence.Forms.FormSection;

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

    internal static ShareVerificationRequest GetShareVerificationRequest(string formVersionId, string shareCode)
    {
        return new ShareVerificationRequest
        {
            FormVersionId = formVersionId,
            ShareCode = shareCode
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

        var organisation = new Organisation
        {
            Id = organisationId,
            Guid = organisationGuid,
            Name = string.Empty,
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = string.Empty
            },
            OrganisationPersons = new List<OrganisationPerson>
        {
            new OrganisationPerson
            {
                Person = new Person
                {
                    Guid = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com"
                },
                Organisation = null
            }
        },
            Identifiers = new List<Organisation.Identifier>
        {
            new Organisation.Identifier
            {
                Scheme = "CDP-PPON",
                IdentifierId = "123456789",
                LegalName = "Test Organisation Ltd.",
                Primary = true
            }
        },
        Addresses = new List<Organisation.OrganisationAddress>
        {
            new Organisation.OrganisationAddress
            {
                Type = CO.CDP.OrganisationInformation.AddressType.Registered,
                Address = new Address
                {
                    StreetAddress = "123 Example Street",
                    Locality = "Example Town",
                    Region = "Example Region",
                    PostalCode = "EX1 1EX",
                    CountryName = "Example Country",
                    Country = "EX"
                }
            }
        }
    };

        return new OrganisationInformation.Persistence.Forms.SharedConsent()
        {
            Guid = formId,
            OrganisationId = organisation.Id,
            Organisation = organisation,
            FormId = form.Id,
            Form = form,
            AnswerSets = new List<FormAnswerSet> { },
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTime.UtcNow,
            FormVersionId = string.Empty,
            ShareCode = string.Empty
        };
    }


    internal static List<OrganisationInformation.Persistence.Forms.SharedConsent> GetSharedConsents(int organisationId, Guid organisationGuid, Guid formId)
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

        var orgnisation = new Organisation
        {
            Id = organisationId,
            Guid = organisationGuid,
            Name = string.Empty,
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = string.Empty
            },
            OrganisationPersons = new List<OrganisationPerson>
        {
            new OrganisationPerson
            {
                Person = new Person
                {
                    Guid = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com"
                },
                Organisation = null //TODO: verfiy
            }
        },
            // Initialize Identifiers here
            Identifiers = new List<Organisation.Identifier>
        {
            new Organisation.Identifier
            {
                Scheme = "CDP-PPON",
                IdentifierId = "123456789",
                LegalName = "Test Organisation Ltd.",
                Primary = true
            }
        }
        };

        return new List<OrganisationInformation.Persistence.Forms.SharedConsent>
        {
            new OrganisationInformation.Persistence.Forms.SharedConsent()
            {
            Guid = Guid.NewGuid(),
            OrganisationId = orgnisation.Id,
            Organisation = orgnisation,
            FormId = form.Id,
            Form = form,
            AnswerSets = new List<FormAnswerSet> { },
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTime.UtcNow,
            FormVersionId = "V1.0",
            ShareCode = ShareCodeExtensions.GenerateShareCode(),
            UpdatedOn=DateTime.UtcNow.AddHours(-1)
            },
            new OrganisationInformation.Persistence.Forms.SharedConsent()
            {
            Guid = Guid.NewGuid(),
            OrganisationId = orgnisation.Id,
            Organisation = orgnisation,
            FormId = form.Id,
            Form = form,
            AnswerSets = new List<FormAnswerSet> { },
            SubmissionState = SubmissionState.Submitted,
            SubmittedAt = DateTime.UtcNow,
            FormVersionId = "V1.0",
            ShareCode = ShareCodeExtensions.GenerateShareCode(),
            UpdatedOn=DateTime.UtcNow.AddHours(-0.5)
            }
            ,
            new OrganisationInformation.Persistence.Forms.SharedConsent()
            {
            Guid = Guid.NewGuid(),
            OrganisationId = orgnisation.Id,
            Organisation = orgnisation,
            FormId = form.Id,
            Form = form,
            AnswerSets = new List<FormAnswerSet> { },
            SubmissionState = SubmissionState.Submitted,
            SubmittedAt = DateTime.UtcNow,
            FormVersionId = "V1.0",
            ShareCode = ShareCodeExtensions.GenerateShareCode(),
            UpdatedOn=DateTime.UtcNow
            }
        };
    }
}