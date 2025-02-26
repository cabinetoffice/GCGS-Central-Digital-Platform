using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;
using static CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;
using ConnectedOrganisationCategory = CO.CDP.OrganisationInformation.Persistence.ConnectedEntity.ConnectedOrganisationCategory;
using PersistenceForms = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.DataSharing.WebApi.Tests;

internal static class NonEfEntityFactory
{
    private static int _nextQuestionNumber;

    private static int GetQuestionNumber()
    {
        Interlocked.Increment(ref _nextQuestionNumber);

        return _nextQuestionNumber;
    }

    internal static SharedConsentDS GetSharedConsent(Guid organisationGuid, Guid formId)
    {
        var form = new FormDS
        {
            Name = "Standard Questions",
            Version = "1.0",
            IsRequired = true,
            //Sections = new List<FormSectionDS> {
            //    new FormSectionDS
            //    {
            //        Id = 1,
            //        Guid = Guid.NewGuid(),
            //        Title = "General Information",
            //        Type = FormSectionType.Standard,
            //        FormId = 1,
            //        Form = new Form { Id = 1, Guid = Guid.NewGuid(), Name = "", Version = "1", Scope = FormScope.SupplierInformation, Sections = new List<FormSection>(), IsRequired = false },
            //        Questions = new List<FormQuestion>
            //        {
            //            new() {
            //                Caption = null,
            //                Description = "Localized_String",
            //                Title = "Localized_String",
            //                Guid = Guid.NewGuid(),
            //                NextQuestion = null,
            //                NextQuestionAlternative = null,
            //                SortOrder = 1,
            //                Section = null!,
            //                IsRequired = false,
            //                Name = "Name",
            //                Options = null!,
            //                Type = PersistenceForms.FormQuestionType.Text
            //            }
            //        },
            //        AllowsMultipleAnswerSets = false,
            //        CheckFurtherQuestionsExempted = false,
            //        DisplayOrder = 1,
            //        Configuration = new FormSectionConfiguration
            //        { SingularSummaryHeading = "", PluralSummaryHeadingFormat = "", AddAnotherAnswerLabel = "", RemoveConfirmationCaption = "", RemoveConfirmationHeading = "", FurtherQuestionsExemptedHeading = "", FurtherQuestionsExemptedHint = "" },
            //        CreatedOn = DateTimeOffset.UtcNow,
            //        UpdatedOn = DateTimeOffset.UtcNow
            //    }
            //}
        };

        var organisation = GivenOrganisation(
            guid: organisationGuid,
            name: "Test Organisation",
            supplierInformation: new SupplierInformationDS()
        );

        var sharedConsent = new SharedConsentDS()
        {
            Guid = formId,
            Organisation = organisation,
            Form = form,
            AnswerSets = new List<FormAnswerSetDS> { },
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTime.UtcNow,
            ShareCode = string.Empty
        };
        foreach (var questionsAndAnswer in GivenQuestionsAndAnswers(sharedConsent, form))
        {
            //sharedConsent.Form.Sections.Add(questionsAndAnswer.Section);
            sharedConsent.AnswerSets.Add(questionsAndAnswer);
        }

        return sharedConsent;
    }

    internal static List<ConnectedEntityDS> GetMockIndividuals()
    {
        var mockPersons = new List<ConnectedEntityDS>
            {
                new ConnectedEntityDS
                {
                    Guid = Guid.NewGuid(),
                    EntityType = OrganisationInformation.ConnectedEntityType.Individual,
                    IndividualOrTrust = new ConnectedIndividualTrustDS
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Category = ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndiv
                    }
                }
            };

        return mockPersons;
    }

    internal static List<ConnectedEntityDS> GetMockAdditionalEntities()
    {
        var mockEntities = new List<ConnectedEntityDS>
            {
                new ConnectedEntityDS
                {
                    Guid = Guid.NewGuid(),
                    EntityType = OrganisationInformation.ConnectedEntityType.Organisation,
                    Organisation = new ConnectedOrganisationDS
                    {
                        Name = "Acme Group Ltd",
                        Category = ConnectedOrganisationCategory.RegisteredCompany
                    }
                }
            };

        return mockEntities;
    }

    internal static List<ConnectedEntityDS> GetMockTrustsOrTrustees()
    {
        var mockPersons = new List<ConnectedEntityDS>
            {
                new ConnectedEntityDS
                {
                    Guid = Guid.NewGuid(),
                    EntityType = OrganisationInformation.ConnectedEntityType.TrustOrTrustee,
                    IndividualOrTrust = new ConnectedIndividualTrustDS
                    {
                        FirstName = "John",
                        LastName = "Smith",
                        Category = ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust
                    }
                }
            };

        return mockPersons;
    }

    internal static LegalFormDS GetLegalForm()
    {
        var mockLegalForm = new LegalFormDS
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

    public static OrganisationDS GivenOrganisation(
        Guid? guid = null,
        string? name = null,
        SupplierInformationDS? supplierInformation = null
    )
    {
        var theGuid = guid ?? Guid.NewGuid();
        var theName = name ?? $"Organisation {theGuid}";
        var organisation = new OrganisationDS
        {
            Guid = theGuid,
            Name = theName,
            Type = OrganisationType.Organisation,

            Identifiers = [
                new IdentifierDS
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = $"{theGuid}",
                    LegalName = "DefaultLegalName",
                    Uri = "https://default.org"
                },
                new IdentifierDS
                {
                    Primary = false,
                    Scheme = "GB-COH",
                    IdentifierId = Guid.NewGuid().ToString(),
                    LegalName = "AnotherLegalName",
                    Uri = "http://example.com"
                }
            ],
            Addresses = [new AddressDS
            {
                Type = AddressType.Registered,
                StreetAddress = "1234 Default St",
                Locality = "Default City",
                Region = "Default Region",
                PostalCode = "EX1 1EX",
                CountryName = "Example Country",
                Country = "EX"
            }],
            ContactPoints = [new ContactPointDS
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

    private static FormSectionDS GivenSection(Guid sectionId, FormDS form, FormSectionType? sectionType = null)
    {
        var formSection = new FormSectionDS
        {
            Id = 1,
            Questions = new List<FormQuestionDS>(),
            Title = "Localized_String",
            Type = sectionType ?? FormSectionType.Declaration
        };

        return formSection;
    }

    public static List<FormAnswerSetDS> GivenQuestionsAndAnswers(
        SharedConsentDS sharedConsent,
        FormDS form)
    {
        var section = GivenSection(Guid.NewGuid(), form);
        var answerSet = new FormAnswerSetDS
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            SectionId = section.Id,
            Section = section,
            Answers = new List<FormAnswerDS>()
        };

        var questions = new List<FormQuestionDS>
                {
                    new FormQuestionDS
                    {
                        Id = 1,
                        Guid = Guid.NewGuid(),
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "The financial information you will need.",
                        Description = "You will need to upload accounts or statements for your 2 most recent financial years. If you do not have 2 years, you can upload your most recent financial year. You will need to enter the financial year end date for the information you upload.",
                        Type = PersistenceForms.FormQuestionType.NoInput,
                        IsRequired = true,
                        Options = new FormQuestionOptions(),
                        Section = section,
                        SortOrder = 1
                    },
                    new FormQuestionDS
                    {
                        Id = 2,
                        Guid = Guid.NewGuid(),
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "Localized_String",
                        Description = "Localized_String",
                        Type = PersistenceForms.FormQuestionType.YesOrNo,
                        IsRequired = true,
                        Options = new FormQuestionOptions(),
                        Section = section,
                        SortOrder = 2
                    },
                    new FormQuestionDS
                    {
                        Id = 2,
                        Guid  = Guid.NewGuid(),
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "Upload your accounts",
                        Description = "Upload your most recent 2 financial years. If you do not have 2, upload your most recent financial year.",
                        Type = PersistenceForms.FormQuestionType.FileUpload,
                        IsRequired = true,
                        Options = new FormQuestionOptions(),
                        Section = section,
                        SortOrder = 3
                    },
                    new FormQuestionDS
                    {
                        Id = 3,
                        Guid  = Guid.NewGuid(),
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "What is the financial year end date for the information you uploaded?",
                        Description = String.Empty,
                        Type = PersistenceForms.FormQuestionType.Date,
                        IsRequired = true,
                        Options = new FormQuestionOptions(),
                        Section = section,
                        SortOrder = 4
                    },
                    new FormQuestionDS
                    {
                        Id = 4,
                        Guid  = Guid.NewGuid(),
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "Check your answers",
                        Type = PersistenceForms.FormQuestionType.CheckYourAnswers,
                        Description = String.Empty,
                        IsRequired = true,
                        Options = new FormQuestionOptions(),
                        Section = section,
                        SortOrder = 5
                    },
                    new FormQuestionDS
                    {
                        Id = 5,
                        Guid  = Guid.NewGuid(),
                        Name = "_Section0" + GetQuestionNumber(),
                        Title = "Enter your postal address",
                        Description = string.Empty,
                        Type = PersistenceForms.FormQuestionType.Address,
                        IsRequired = true,
                        Options = new FormQuestionOptions(),
                        Section = section,
                        SortOrder = 6
                    }
                };
        section.Questions = questions;

        var exclusionSection = GivenSection(Guid.NewGuid(), form, FormSectionType.Exclusions);

        var formAnswers = new List<FormAnswerSetDS>
            {
                new() {
                    Id = 1,
                    Guid = Guid.NewGuid(),
                    SectionId = exclusionSection.Id,
                    Section = exclusionSection,
                    Answers =
                    [
                        new FormAnswerDS
                        {
                            QuestionId = questions[0].Id,
                            Question = questions[0],
                            FormAnswerSetId = answerSet.Id,
                            FormAnswerSet = answerSet,
                            BoolValue = true
                        },
                         new FormAnswerDS
                        {
                            QuestionId = questions[1].Id,
                            Question = questions[1],
                            FormAnswerSetId = answerSet.Id,
                            FormAnswerSet = answerSet,
                            OptionValue="yes"
                        },
                        new  FormAnswerDS
                        {
                            QuestionId = questions[2].Id,
                            Question = questions[2],
                            FormAnswerSetId = answerSet.Id,
                            FormAnswerSet = answerSet,
                            TextValue="a_dummy_file.pdf"
                        },
                        new  FormAnswerDS
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