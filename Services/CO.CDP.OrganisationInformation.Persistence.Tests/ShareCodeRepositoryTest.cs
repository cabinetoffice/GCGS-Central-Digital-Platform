using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class ShareCodeRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{

    [Fact]
    public async Task GetSharedConsentDraftAsync_WhenSharedConsentDoesNotExist_ReturnsNull()
    {
        using var repository = ShareCodeRepository();

        var foundConsent = await repository.GetSharedConsentDraftAsync(Guid.NewGuid(), Guid.NewGuid());

        foundConsent.Should().BeNull();
    }

    [Fact]
    public async Task GetSharedConsentDraftAsync_WhenItDoesExist_ReturnsIt()
    {
        var sharedConsent = GivenSharedConsent();

        await using var context = postgreSql.OrganisationInformationContext();
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = ShareCodeRepository();

        var found = await repository.GetSharedConsentDraftAsync(sharedConsent.Form.Guid, sharedConsent.Organisation.Guid);

        found.Should().NotBeNull();
        found.As<SharedConsent>().OrganisationId.Should().Be(sharedConsent.OrganisationId);
        found.As<SharedConsent>().SubmissionState.Should().Be(SubmissionState.Draft);
        found.As<SharedConsent>().BookingReference.Should().BeNull();
    }

    [Fact]
    public async Task GetShareCodesAsync_WhenCodesDoesNotExist_ReturnsEmptyList()
    {
        using var repository = ShareCodeRepository();

        var foundSection = await repository.GetShareCodesAsync(Guid.NewGuid());

        foundSection.Should().BeEmpty();
    }

    [Fact]
    public async Task GetShareCodesAsync_WhenCodesExist_ReturnsList()
    {
        var sharedConsent = GivenSharedConsent();
        Random rand = new Random();
        var bookingref = rand.Next(10000000, 99999999).ToString();

        sharedConsent.BookingReference = bookingref;
        sharedConsent.SubmissionState = SubmissionState.Submitted;
        sharedConsent.SubmittedAt = DateTime.UtcNow;

        await using var context = postgreSql.OrganisationInformationContext();
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = ShareCodeRepository();

        var found = await repository.GetShareCodesAsync(sharedConsent.Organisation.Guid);

        found.Should().NotBeEmpty();
        found.Should().HaveCount(1);

        found.First().As<SharedConsent>().OrganisationId.Should().Be(sharedConsent.OrganisationId);
        found.First().As<SharedConsent>().SubmissionState.Should().Be(SubmissionState.Submitted);
        found.First().As<SharedConsent>().BookingReference.Should().Be(bookingref);
    }


    [Fact]
    public async Task GetByShareCode_WhenShareCodeDoesNotExist_ReturnsNull()
    {
        using var repository = ShareCodeRepository();

        var foundConsent = await repository.GetByShareCode("NONEXISTENTCODE");

        foundConsent.Should().BeNull();
    }

    [Fact]
    public async Task GetByShareCode_WhenShareCodeExists_ReturnsSharedConsent()
    {
        var form = GivenForm(Guid.NewGuid());
        var section = GivenSection(Guid.NewGuid(), form);
        var question = GivenYesOrNoQuestion(section);
        var sharedConsent = GivenSharedConsent(form);
        var answerSet = GivenAnswerSet(sharedConsent, section);
        var answer = GivenAnswer(question, answerSet);

        sharedConsent.SubmissionState = SubmissionState.Submitted;
        sharedConsent.SubmittedAt = DateTime.UtcNow;
        var shareCode = "EXISTENTCODE";
        sharedConsent.BookingReference = shareCode;

        await using var context = postgreSql.OrganisationInformationContext();
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = ShareCodeRepository();

        var foundConsent = await repository.GetByShareCode(shareCode);

        foundConsent.Should().NotBeNull();
        foundConsent!.BookingReference.Should().Be(shareCode);
        foundConsent.Organisation.Name.Should().StartWith("Acme Corporation");
        foundConsent.Organisation.Identifiers.Should().ContainSingle(i => i.LegalName == "Acme Corporation Ltd");
        foundConsent.Organisation.ContactPoints.Should().ContainSingle(cp => cp.Name == "Procurement Team");

        var registeredAddress = foundConsent.Organisation.Addresses.FirstOrDefault(a => a.Type == AddressType.Registered);
        registeredAddress.Should().NotBeNull();
        registeredAddress!.Address.Should().NotBeNull();
        registeredAddress!.Address.StreetAddress.Should().Be("82 St. John’s Road");
        registeredAddress!.Address.PostalCode.Should().Be("CH43 7UR");

        foundConsent.AnswerSets.Should().ContainSingle();

        var retrievedAnswerSet = foundConsent.AnswerSets.First();
        retrievedAnswerSet.Answers.Should().ContainSingle(a => a.BoolValue == true && a.Question.Title == "Yes or no?");

        var retrievedForm = foundConsent.Form;
        retrievedForm.Should().NotBeNull();
        retrievedForm.Name.Should().Be("Test Form");
        retrievedForm.Sections.Should().ContainSingle(s => s.Title == "Test Section");

        var retrievedSection = retrievedForm.Sections.First();
        retrievedSection.Questions.Should().ContainSingle(q => q.Title == "Yes or no?");
    }

    private static SharedConsent GivenSharedConsent(
        Form? form = null,
        Organisation? organisation = null)
    {
        form ??= GivenForm(Guid.NewGuid());
        organisation ??= GivenOrganisation();

        return new SharedConsent
        {
            Guid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            FormId = form.Id,
            Form = form,
            AnswerSets = [],
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = null,
            FormVersionId = "202404",
            BookingReference = null
        };
    }

    private static Form GivenForm(Guid formId)
    {
        return new Form
        {
            Guid = formId,
            Name = "Test Form",
            Version = "1.0",
            IsRequired = true,
            Scope = FormScope.SupplierInformation,
            Sections = new List<FormSection>()
        };
    }

    private static Organisation GivenOrganisation()
    {
        return new Organisation
        {
            Guid = Guid.NewGuid(),
            Name = "Acme Corporation " + Guid.NewGuid().ToString(), // Ensure unique name
            Tenant = GivenTenant(),
            Identifiers = new List<Organisation.Identifier>
        {
            new Organisation.Identifier
            {
                IdentifierId = Guid.NewGuid().ToString(), // Generate a unique IdentifierId
                Scheme = "CDP-PPON",
                LegalName = "Acme Corporation Ltd",
                Uri = "https://cdp.cabinetoffice.gov.uk/organisations/" + Guid.NewGuid().ToString(), // Ensure unique URI
                Primary = true
            }
        },
            ContactPoints = new List<Organisation.ContactPoint>
        {
            new Organisation.ContactPoint
            {
                Name = "Procurement Team",
                Email = "info@example.com",
                Telephone = "+441234567890"
            }
        },
            Addresses = new List<Organisation.OrganisationAddress>
        {
            new Organisation.OrganisationAddress
            {
                Type = AddressType.Registered,
                Address = new OrganisationInformation.Persistence.Address
                {
                    StreetAddress = "82 St. John’s Road",
                    Locality = "CHESTER",
                    Region = "Lancashire",
                    PostalCode = "CH43 7UR",
                    CountryName = "United Kingdom"
                }
            }
        }
        };
    }

    private static Tenant GivenTenant()
    {
        return new Tenant
        {
            Guid = Guid.NewGuid(),
            Name = "Acme Tenant " + Guid.NewGuid().ToString()
        };
    }

    private static FormSection GivenSection(Guid sectionId, Form form)
    {
        return new FormSection
        {
            Guid = sectionId,
            FormId = form.Id,
            Form = form,
            Questions = new List<FormQuestion>(),
            Title = "Test Section",
            Type = FormSectionType.Standard,
            AllowsMultipleAnswerSets = true,
            Configuration = new FormSectionConfiguration
            {
                PluralSummaryHeadingFormat = "You have added {0} files",
                SingularSummaryHeading = "You have added 1 file",
                AddAnotherAnswerLabel = "Add another file?",
                RemoveConfirmationCaption = "Economic and Financial Standing",
                RemoveConfirmationHeading = "Are you sure you want to remove this file?"
            }
        };
    }

    private static FormAnswer GivenAnswer(FormQuestion question, FormAnswerSet answerSet)
    {
        var answer = new FormAnswer
        {
            Guid = Guid.NewGuid(),
            Question = question,
            FormAnswerSet = answerSet,
            BoolValue = true
        };
        answerSet.Answers.Add(answer);
        return answer;
    }

    private static FormAnswerSet GivenAnswerSet(SharedConsent sharedConsent, FormSection section)
    {
        var answerSet = new FormAnswerSet
        {
            Guid = Guid.NewGuid(),
            SharedConsentId = sharedConsent.Id,
            SharedConsent = sharedConsent,
            SectionId = section.Id,
            Section = section,
            Answers = [],
        };
        sharedConsent.AnswerSets.Add(answerSet);
        return answerSet;
    }

    private static FormQuestion GivenYesOrNoQuestion(FormSection section)
    {
        var question = new FormQuestion
        {
            Guid = Guid.NewGuid(),
            Section = section,
            Type = FormQuestionType.YesOrNo,
            IsRequired = true,
            Title = "Yes or no?",
            Description = "Please answer.",
            NextQuestion = null,
            NextQuestionAlternative = null,
            Caption = null,
            Options = new()
        };
        section.Questions.Add(question);
        return question;
    }

    private IShareCodeRepository ShareCodeRepository()
    {
        return new DatabaseShareCodeRepository(postgreSql.OrganisationInformationContext());
    }
}