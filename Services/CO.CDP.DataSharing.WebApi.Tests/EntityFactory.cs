using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using static CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;
using static CO.CDP.OrganisationInformation.Persistence.Organisation;
using ConnectedEntityType = CO.CDP.OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityType;
using ConnectedOrganisationCategory = CO.CDP.OrganisationInformation.Persistence.ConnectedEntity.ConnectedOrganisationCategory;
using PersistenceForms = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.DataSharing.WebApi.Tests;

internal static class EntityFactory
{
    private static int _nextQuestionNumber;

    private static int GetQuestionNumber()
    {
        Interlocked.Increment(ref _nextQuestionNumber);

        return _nextQuestionNumber;
    }

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

    internal static PersistenceForms.SharedConsent GetSharedConsent(int organisationId, Guid organisationGuid, Guid formId)
    {
        var form = new PersistenceForms.Form
        {
            Guid = formId,
            Name = "Standard Questions",
            Version = "1.0",
            IsRequired = true,
            Scope = PersistenceForms.FormScope.SupplierInformation,
            Sections = new List<PersistenceForms.FormSection> { }
        };

        var organisation = GivenOrganisation(
            guid: organisationGuid,
            name: "Test Organisation",
            tenant: new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = "Test Tenant"
            }
        );

        organisation.Id = organisationId;

        var sharedConsent = new PersistenceForms.SharedConsent()
        {
            Guid = formId,
            OrganisationId = organisation.Id,
            Organisation = organisation,
            FormId = form.Id,
            Form = form,
            AnswerSets = new List<PersistenceForms.FormAnswerSet> { },
            SubmissionState = PersistenceForms.SubmissionState.Draft,
            SubmittedAt = DateTime.UtcNow,
            FormVersionId = string.Empty,
            ShareCode = string.Empty
        };
        foreach (var questionsAndAnswer in GivenQuestionsAndAnswers(sharedConsent, form))
        {
            sharedConsent.Form.Sections.Add(questionsAndAnswer.Section);
            sharedConsent.AnswerSets.Add(questionsAndAnswer);
        }

        return sharedConsent;
    }

    internal static List<ConnectedEntity> GetMockIndividuals()
    {
        var mockPersons = new List<ConnectedEntity>
            {
                new ConnectedEntity
                {
                    Guid = Guid.NewGuid(),
                    EntityType = ConnectedEntityType.Individual,
                    IndividualOrTrust = new ConnectedEntity.ConnectedIndividualTrust
                    {
                        Id = 1,
                        FirstName = "John",
                        LastName = "Doe",
                        Category = ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual,
                        CreatedOn = DateTimeOffset.UtcNow,
                        UpdatedOn = DateTimeOffset.UtcNow
                    },
                    SupplierOrganisation = GivenOrganisation(),
                    CreatedOn = DateTimeOffset.UtcNow,
                    UpdatedOn = DateTimeOffset.UtcNow
                }
            };

        return mockPersons;
    }

    internal static List<ConnectedEntity> GetMockAdditionalEntities()
    {
        var mockEntities = new List<ConnectedEntity>
            {
                new ConnectedEntity
                {
                    Guid = Guid.NewGuid(),
                    EntityType = ConnectedEntityType.Organisation,
                    Organisation = new ConnectedEntity.ConnectedOrganisation
                    {
                        Id = 1,
                        Name = "Acme Group Ltd",
                        Category = ConnectedOrganisationCategory.RegisteredCompany,
                        CreatedOn = DateTimeOffset.UtcNow,
                        UpdatedOn = DateTimeOffset.UtcNow
                    },
                    SupplierOrganisation = GivenOrganisation(),
                    CreatedOn = DateTimeOffset.UtcNow,
                    UpdatedOn = DateTimeOffset.UtcNow
                }
            };

        return mockEntities;
    }

    internal static List<ConnectedEntity> GetMockTrustsOrTrustees()
    {
        var mockPersons = new List<ConnectedEntity>
            {
                new ConnectedEntity
                {
                    Guid = Guid.NewGuid(),
                    EntityType = ConnectedEntityType.TrustOrTrustee,
                    IndividualOrTrust = new ConnectedEntity.ConnectedIndividualTrust
                    {
                        Id = 2,
                        FirstName = "John",
                        LastName = "Smith",
                        Category = ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust,
                        CreatedOn = DateTimeOffset.UtcNow,
                        UpdatedOn = DateTimeOffset.UtcNow
                    },
                    SupplierOrganisation = GivenOrganisation(),
                    CreatedOn = DateTimeOffset.UtcNow,
                    UpdatedOn = DateTimeOffset.UtcNow
                }
            };

        return mockPersons;
    }

    internal static Organisation.LegalForm GetLegalForm()
    {
        var mockLegalForm = new Organisation.LegalForm
        {
            RegisteredUnderAct2006 = false,
            RegisteredLegalForm = "Registered Legal Form 1",
            LawRegistered = "Law Registered 1",
            RegistrationDate = DateTimeOffset.UtcNow
        };

        return mockLegalForm;
    }

    internal static IList<OperationType> GetOperationTypes()
    {
        var mockOperationType = new List<OperationType> { OperationType.SmallOrMediumSized };

        return mockOperationType;
    }

    public static Organisation GivenOrganisation(
        Guid? guid = null,
        Tenant? tenant = null,
        string? name = null,
        List<Organisation.Identifier>? identifiers = null,
        List<OrganisationAddress>? addresses = null,
        Organisation.ContactPoint? contactPoint = null,
        List<PartyRole>? roles = null,
        List<(Person, List<string>)>? personsWithScope = null,
        BuyerInformation? buyerInformation = null,
        Organisation.SupplierInformation? supplierInformation = null
    )
    {
        var theGuid = guid ?? Guid.NewGuid();
        var theName = name ?? $"Organisation {theGuid}";
        var organisation = new Organisation
        {
            Guid = theGuid,
            Name = theName,
            Tenant = tenant ?? GivenTenant(name: theName),

            Identifiers = identifiers ??
            [
                new Organisation.Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = $"{theGuid}",
                    LegalName = "DefaultLegalName",
                    Uri = "https://default.org"
                },
                new Organisation.Identifier
                {
                    Primary = false,
                    Scheme = "GB-COH",
                    IdentifierId = Guid.NewGuid().ToString(),
                    LegalName = "AnotherLegalName",
                    Uri = "http://example.com"
                }
            ],
            Addresses = addresses ?? [new OrganisationAddress
            {
                Type = AddressType.Registered,
                Address = new OrganisationInformation.Persistence.Address
                {
                    StreetAddress = "1234 Default St",
                    Locality = "Default City",
                    Region = "Default Region",
                    PostalCode = "EX1 1EX",
                    CountryName = "Example Country",
                    Country = "EX"
                }
            }],
            ContactPoints = contactPoint == null ? [new Organisation.ContactPoint
            {
                Name = "Default Contact",
                Email = "contact@default.org",
                Telephone = "123-456-7890",
                Url = "https://contact.default.org"
            }] : [contactPoint],
            Roles = roles ?? [PartyRole.Buyer],
            BuyerInfo = buyerInformation,
            SupplierInfo = supplierInformation
        };

        foreach (var personWithScope in personsWithScope ?? [])
        {
            organisation.OrganisationPersons.Add(
                new OrganisationPerson
                {
                    Person = personWithScope.Item1,
                    Organisation = organisation,
                    Scopes = personWithScope.Item2
                }
            );
        }
        return organisation;
    }

    public static Tenant GivenTenant(
        Guid? guid = null,
        string? name = null,
        Organisation? organisation = null,
        Person? person = null)
    {
        var theGuid = guid ?? Guid.NewGuid();
        var theName = name ?? $"Stefan {theGuid}";
        var tenant = new Tenant
        {
            Guid = theGuid,
            Name = theName
        };

        if (organisation != null)
            tenant.Organisations.Add(organisation);

        if (person != null)
            tenant.Persons.Add(person);

        return tenant;
    }

    private static PersistenceForms.FormSection GivenSection(Guid sectionId, PersistenceForms.Form form, PersistenceForms.FormSectionType? sectionType = null)
    {
        var formSection = new PersistenceForms.FormSection
        {
            Guid = sectionId,
            FormId = form.Id,
            Form = form,
            Questions = new List<PersistenceForms.FormQuestion>(),
            Title = "FinancialInformation_SectionTitle",
            Type = sectionType ?? PersistenceForms.FormSectionType.Declaration,
            AllowsMultipleAnswerSets = true,
            CheckFurtherQuestionsExempted = false,
            DisplayOrder = 1,
            Configuration = new PersistenceForms.FormSectionConfiguration
            {
                PluralSummaryHeadingFormat = "You have added {0} files",
                SingularSummaryHeading = "You have added 1 file",
                AddAnotherAnswerLabel = "Add another file?",
                RemoveConfirmationCaption = "Economic and Financial Standing",
                RemoveConfirmationHeading = "Are you sure you want to remove this file?"
            }
        };

        return formSection;
    }

    public static List<PersistenceForms.FormAnswerSet> GivenQuestionsAndAnswers(
        PersistenceForms.SharedConsent sharedConsent,
        PersistenceForms.Form form)
    {
        var section = GivenSection(Guid.NewGuid(), form);
        var answerSet = new PersistenceForms.FormAnswerSet
        {
            Guid = Guid.NewGuid(),
            SharedConsentId = default,
            SharedConsent = sharedConsent,
            SectionId = section.Id,
            Section = section,
            Answers = new List<PersistenceForms.FormAnswer>() { },
            FurtherQuestionsExempted = false,
        };

        var questions = new List<PersistenceForms.FormQuestion>
                {
                    new PersistenceForms.FormQuestion
                    {
                        Id = 1,
                        Guid = Guid.NewGuid(),
                        Caption = "Page caption",
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "The financial information you will need.",
                        Description = "You will need to upload accounts or statements for your 2 most recent financial years. If you do not have 2 years, you can upload your most recent financial year. You will need to enter the financial year end date for the information you upload.",
                        Type = PersistenceForms.FormQuestionType.NoInput,
                        IsRequired = true,
                        Options = new PersistenceForms.FormQuestionOptions(),
                        NextQuestion = null,
                        NextQuestionAlternative = null,
                        CreatedOn = DateTimeOffset.UtcNow,
                        UpdatedOn = DateTimeOffset.UtcNow,
                        Section = section,
                        SortOrder = 1
                    },
                    new PersistenceForms.FormQuestion
                    {
                        Id = 2,
                        Guid = Guid.NewGuid(),
                        Caption = "Page caption",
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "Were your accounts audited?",
                        Description = String.Empty,
                        Type = PersistenceForms.FormQuestionType.YesOrNo,
                        IsRequired = true,
                        Options = new PersistenceForms.FormQuestionOptions(),
                        NextQuestion = null,
                        NextQuestionAlternative = null,
                        CreatedOn = DateTimeOffset.UtcNow,
                        UpdatedOn = DateTimeOffset.UtcNow,
                        Section = section,
                        SortOrder = 2
                    },
                    new PersistenceForms.FormQuestion
                    {
                        Id = 2,
                        Guid  = Guid.NewGuid(),
                        Caption = "Page caption",
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "Upload your accounts",
                        Description = "Upload your most recent 2 financial years. If you do not have 2, upload your most recent financial year.",
                        Type = PersistenceForms.FormQuestionType.FileUpload,
                        IsRequired = true,
                        Options = new PersistenceForms.FormQuestionOptions(),
                        NextQuestion = null,
                        NextQuestionAlternative = null,
                        CreatedOn = DateTimeOffset.UtcNow,
                        UpdatedOn = DateTimeOffset.UtcNow,
                        Section = section,
                        SortOrder = 3
                    },
                    new PersistenceForms.FormQuestion
                    {
                        Id = 3,
                        Guid  = Guid.NewGuid(),
                        Caption = "Page caption",
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "What is the financial year end date for the information you uploaded?",
                        Description = String.Empty,
                        Type = PersistenceForms.FormQuestionType.Date,
                        IsRequired = true,
                        Options = new PersistenceForms.FormQuestionOptions(),
                        NextQuestion = null,
                        NextQuestionAlternative = null,
                        CreatedOn = DateTimeOffset.UtcNow,
                        UpdatedOn = DateTimeOffset.UtcNow,
                        Section = section,
                        SortOrder = 4
                    },
                    new PersistenceForms.FormQuestion
                    {
                        Id = 4,
                        Guid  = Guid.NewGuid(),
                        Caption = "Page caption",
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "Check your answers",
                        Type = PersistenceForms.FormQuestionType.CheckYourAnswers,
                        Description = String.Empty,
                        IsRequired = true,
                        Options = new PersistenceForms.FormQuestionOptions(),
                        NextQuestion = null,
                        NextQuestionAlternative = null,
                        CreatedOn = DateTimeOffset.UtcNow,
                        UpdatedOn = DateTimeOffset.UtcNow,
                        Section = section,
                        SortOrder = 5
                    },
                    new PersistenceForms.FormQuestion
                    {
                        Id = 5,
                        Guid  = Guid.NewGuid(),
                        Caption = "Page caption",
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "Enter your postal address",
                        Description = string.Empty,
                        Type = PersistenceForms.FormQuestionType.Address,
                        IsRequired = true,
                        Options = new PersistenceForms.FormQuestionOptions(),
                        NextQuestion = null,
                        NextQuestionAlternative = null,
                        CreatedOn = DateTimeOffset.UtcNow,
                        UpdatedOn = DateTimeOffset.UtcNow,
                        Section = section,
                        SortOrder = 6
                    }
                };
        section.Questions = questions;

        var exclusionSection = GivenSection(Guid.NewGuid(), form, PersistenceForms.FormSectionType.Exclusions);

        var formAnswers = new List<PersistenceForms.FormAnswerSet>
            {
                new PersistenceForms.FormAnswerSet
                {
                    Guid = Guid.NewGuid(),
                    SharedConsentId = default,
                    SharedConsent = sharedConsent,
                    SectionId = exclusionSection.Id,
                    Section = exclusionSection,
                    Answers =
                    [
                        new PersistenceForms.FormAnswer
                        {
                            Guid = Guid.NewGuid(),
                            QuestionId = questions[0].Id,
                            Question = questions[0],
                            FormAnswerSetId = answerSet.Id,
                            FormAnswerSet = answerSet,
                            BoolValue = true
                        },
                          new PersistenceForms.FormAnswer
                        {
                            Guid = Guid.NewGuid(),
                            QuestionId = questions[1].Id,
                            Question = questions[1],
                            FormAnswerSetId = answerSet.Id,
                            FormAnswerSet = answerSet,
                            OptionValue="yes"
                        },
                            new  PersistenceForms.FormAnswer
                        {
                            Guid = Guid.NewGuid(),
                            QuestionId = questions[2].Id,
                            Question = questions[2],
                            FormAnswerSetId = answerSet.Id,
                            FormAnswerSet = answerSet,
                            TextValue="a_dummy_file.pdf"
                        },
                        new  PersistenceForms.FormAnswer
                        {
                            Guid = Guid.NewGuid(),
                            QuestionId = questions[3].Id,
                            Question = questions[3],
                            FormAnswerSetId = answerSet.Id,
                            FormAnswerSet = answerSet,
                            DateValue=DateTime.Now
                        }
                    ],
                    FurtherQuestionsExempted = false
                },
                answerSet
            };


        return formAnswers;
    }
}