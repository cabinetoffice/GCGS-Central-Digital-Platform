using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;
using FluentAssertions.Common;
using static CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;

namespace CO.CDP.DataSharing.WebApi.Tests;

internal static class NonEfEntityFactory
{
    private static int _nextQuestionNumber;

    private static int GetQuestionNumber()
    {
        Interlocked.Increment(ref _nextQuestionNumber);

        return _nextQuestionNumber;
    }

    internal static SharedConsentNonEf GetSharedConsent(Guid organisationGuid, Guid formId)
    {
        var form = new FormNonEf
        {
            Name = "Standard Questions",
            Version = "1.0",
            IsRequired = true
        };

        var organisation = GivenOrganisation(
            guid: organisationGuid,
            name: "Test Organisation",
            supplierInformation: new SupplierInformationNonEf()
        );

        var sharedConsent = new SharedConsentNonEf()
        {
            Guid = formId,
            Organisation = organisation,
            Form = form,
            AnswerSets = new List<FormAnswerSetNonEf> { },
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTime.UtcNow,
            ShareCode = string.Empty
        };
        foreach (var questionsAndAnswer in GivenQuestionsAndAnswers(sharedConsent, form))
        {
            sharedConsent.AnswerSets.Add(questionsAndAnswer);
        }

        return sharedConsent;
    }

    internal static List<ConnectedEntityNonEf> GetMockIndividuals()
    {
        var mockPersons = new List<ConnectedEntityNonEf>
            {
                new ConnectedEntityNonEf
                {
                    Guid = new Guid("8f127354-9777-44d3-93dd-a7437e0cc552"),
                    EntityType = ConnectedEntityType.Individual,
                    IndividualOrTrust = new ConnectedIndividualTrustNonEf
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Category = ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndiv,
                        ConnectedType = ConnectedPersonType.Individual,
                        ControlCondition  = [
                            ControlCondition.HasOtherSignificantInfluenceOrControl,
                            ControlCondition.HasVotingRights
                            ],
                        DateOfBirth = DateTime.Today.AddYears(30),
                        Nationality = "British",
                        ResidentCountry = "United Kingdom"
                    },
                    HasCompanyHouseNumber = true,
                    CompanyHouseNumber = "TestOrg123",
                    OverseasCompanyNumber = "Oversears123",
                    RegisteredDate = DateTime.Today.ToDateTimeOffset(),
                    RegisterName = "Approved By Trade Association",
                    EndDate = DateTime.Today.AddDays(5).ToDateTimeOffset(),
                    Addresses = [ new AddressNonEf {
                        Type = AddressType.Registered,
                        StreetAddress = "1234 Default St",
                        Locality = "Default City",
                        Region = "Default Region",
                        PostalCode = "EX1 1EX",
                        CountryName = "Example Country",
                        Country = "EX"
                    }]
                }
            };

        return mockPersons;
    }

    internal static List<ConnectedEntityNonEf> GetMockAdditionalEntities()
    {
        var mockEntities = new List<ConnectedEntityNonEf>
            {
                new ConnectedEntityNonEf
                {
                    Guid = new Guid("57b1895f-11bb-4cd4-ae38-82f38a70237b"),
                    EntityType = ConnectedEntityType.Organisation,
                    Organisation = new ConnectedOrganisationNonEf
                    {
                        Name = "Acme Group Ltd",
                        Category = ConnectedOrganisationCategory.RegisteredCompany,
                        ControlCondition  = [
                            ControlCondition.CanAppointOrRemoveDirectors,
                            ControlCondition.HasVotingRights,
                            ControlCondition.OwnsShares,
                            ],
                        InsolvencyDate = DateTime.Today,
                        LawRegistered = "Trade Law 2024",
                        RegisteredLegalForm = "Trade Association",
                    },
                    HasCompanyHouseNumber = true,
                    CompanyHouseNumber = "TestOrg456",
                    OverseasCompanyNumber = "Oversears456",
                    RegisteredDate = DateTime.Today.ToDateTimeOffset(),
                    RegisterName = "Gov Authority of UK",
                    EndDate = DateTime.Today.AddMonths(5).ToDateTimeOffset(),
                    Addresses = [ new AddressNonEf {
                        Type = AddressType.Postal,
                        StreetAddress = "1234 New St",
                        Locality = "New City",
                        Region = "New Region",
                        PostalCode = "SF1 1EX",
                        CountryName = "New Country",
                        Country = "NW"
                    }]
                }
            };

        return mockEntities;
    }

    internal static List<ConnectedEntityNonEf> GetMockTrustsOrTrustees()
    {
        var mockPersons = new List<ConnectedEntityNonEf>
            {
                new ConnectedEntityNonEf
                {
                    Guid = new Guid("d3d35f4b-953a-4620-8771-fd245d55dd92"),
                    EntityType = ConnectedEntityType.TrustOrTrustee,
                    IndividualOrTrust = new ConnectedIndividualTrustNonEf
                    {
                        FirstName = "John",
                        LastName = "Smith",
                        Category = ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust,
                        ConnectedType = ConnectedPersonType.TrustOrTrustee,
                    }
                }
            };

        return mockPersons;
    }

    internal static LegalFormNonEf GetLegalForm()
    {
        var mockLegalForm = new LegalFormNonEf
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

    public static OrganisationNonEf GivenOrganisation(
        Guid? guid = null,
        string? name = null,
        SupplierInformationNonEf? supplierInformation = null
    )
    {
        var theGuid = guid ?? Guid.NewGuid();
        var theName = name ?? $"Organisation {theGuid}";
        var organisation = new OrganisationNonEf
        {
            Guid = theGuid,
            Name = theName,
            Type = OrganisationType.Organisation,

            Identifiers = [
                new IdentifierNonEf
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = $"{theGuid}",
                    LegalName = "DefaultLegalName",
                    Uri = "https://default.org"
                },
                new IdentifierNonEf
                {
                    Primary = false,
                    Scheme = "GB-COH",
                    IdentifierId = Guid.NewGuid().ToString(),
                    LegalName = "AnotherLegalName",
                    Uri = "http://example.com"
                }
            ],
            Addresses = [new AddressNonEf
            {
                Type = AddressType.Registered,
                StreetAddress = "1234 Default St",
                Locality = "Default City",
                Region = "Default Region",
                PostalCode = "EX1 1EX",
                CountryName = "Example Country",
                Country = "EX"
            },
            new AddressNonEf
            {
                Type = AddressType.Postal,
                StreetAddress = "1234 Default Postal St",
                Locality = "Default City",
                Region = "Default Region",
                PostalCode = "EX1 1EX",
                CountryName = "Example Country",
                Country = "EX"
            }],
            ContactPoints = [new ContactPointNonEf
            {
                Name = "Default Contact",
                Email = "contact@default.org",
                Telephone = "123-456-7890",
                Url = "https://contact.default.org"
            }],
            Roles = [PartyRole.Buyer],
            SupplierInfo = supplierInformation
        };

        return organisation;
    }

    private static FormSectionNonEf GivenSection(Guid sectionId, FormNonEf form, FormSectionType? sectionType = null)
    {
        var formSection = new FormSectionNonEf
        {
            Id = 1,
            Questions = new List<FormQuestionNonEf>(),
            Title = "Localized_String",
            Type = sectionType ?? FormSectionType.Declaration
        };

        return formSection;
    }

    public static List<FormAnswerSetNonEf> GivenQuestionsAndAnswers(
        SharedConsentNonEf sharedConsent,
        FormNonEf form)
    {
        var section = GivenSection(Guid.NewGuid(), form);
        var answerSet = new FormAnswerSetNonEf
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            SectionId = section.Id,
            Section = section,
            Answers = new List<FormAnswerNonEf>()
        };

        var questions = new List<FormQuestionNonEf>
                {
                    new FormQuestionNonEf
                    {
                        Id = 1,
                        Guid = Guid.NewGuid(),
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "The financial information you will need.",
                        Description = "You will need to upload accounts or statements for your 2 most recent financial years. If you do not have 2 years, you can upload your most recent financial year. You will need to enter the financial year end date for the information you upload.",
                        Type = FormQuestionType.NoInput,
                        IsRequired = true,
                        Options = new FormQuestionOptions(),
                        Section = section,
                        SortOrder = 1
                    },
                    new FormQuestionNonEf
                    {
                        Id = 2,
                        Guid = Guid.NewGuid(),
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "Localized_String",
                        Description = "Localized_String",
                        Type = FormQuestionType.YesOrNo,
                        IsRequired = true,
                        Options = new FormQuestionOptions(),
                        Section = section,
                        SortOrder = 2
                    },
                    new FormQuestionNonEf
                    {
                        Id = 2,
                        Guid  = Guid.NewGuid(),
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "Upload your accounts",
                        Description = "Upload your most recent 2 financial years. If you do not have 2, upload your most recent financial year.",
                        Type = FormQuestionType.FileUpload,
                        IsRequired = true,
                        Options = new FormQuestionOptions(),
                        Section = section,
                        SortOrder = 3
                    },
                    new FormQuestionNonEf
                    {
                        Id = 3,
                        Guid  = Guid.NewGuid(),
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "What is the financial year end date for the information you uploaded?",
                        Description = String.Empty,
                        Type = FormQuestionType.Date,
                        IsRequired = true,
                        Options = new FormQuestionOptions(),
                        Section = section,
                        SortOrder = 4
                    },
                    new FormQuestionNonEf
                    {
                        Id = 4,
                        Guid  = Guid.NewGuid(),
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "Check your answers",
                        Type = FormQuestionType.CheckYourAnswers,
                        Description = String.Empty,
                        IsRequired = true,
                        Options = new FormQuestionOptions(),
                        Section = section,
                        SortOrder = 5
                    },
                    new FormQuestionNonEf
                    {
                        Id = 5,
                        Guid  = Guid.NewGuid(),
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "Enter your postal address",
                        Description = string.Empty,
                        Type = FormQuestionType.Address,
                        IsRequired = true,
                        Options = new FormQuestionOptions(),
                        Section = section,
                        SortOrder = 6
                    }
                };
        section.Questions = questions;

        var exclusionSection = GivenSection(Guid.NewGuid(), form, FormSectionType.Exclusions);

        var formAnswers = new List<FormAnswerSetNonEf>
            {
                new() {
                    Id = 1,
                    Guid = Guid.NewGuid(),
                    SectionId = exclusionSection.Id,
                    Section = exclusionSection,
                    Answers =
                    [
                        new FormAnswerNonEf
                        {
                            QuestionId = questions[0].Id,
                            Question = questions[0],
                            FormAnswerSetId = answerSet.Id,
                            FormAnswerSet = answerSet,
                            BoolValue = true
                        },
                         new FormAnswerNonEf
                        {
                            QuestionId = questions[1].Id,
                            Question = questions[1],
                            FormAnswerSetId = answerSet.Id,
                            FormAnswerSet = answerSet,
                            OptionValue="yes"
                        },
                        new  FormAnswerNonEf
                        {
                            QuestionId = questions[2].Id,
                            Question = questions[2],
                            FormAnswerSetId = answerSet.Id,
                            FormAnswerSet = answerSet,
                            TextValue="a_dummy_file.pdf"
                        },
                        new  FormAnswerNonEf
                        {
                            QuestionId = questions[3].Id,
                            Question = questions[3],
                            FormAnswerSetId = answerSet.Id,
                            FormAnswerSet = answerSet,
                            DateValue=DateTime.Now
                        }
                    ]
                },
                answerSet
            };


        return formAnswers;
    }
}