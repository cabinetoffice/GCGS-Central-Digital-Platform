using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseShareCodeRepositoryTest(PostgreSqlFixture postgreSql)
    : IClassFixture<PostgreSqlFixture>
{
    private static int _nextQuestionNumber;

    private static int GetQuestionNumber()
    {
        Interlocked.Increment(ref _nextQuestionNumber);

        return _nextQuestionNumber;
    }

    [Fact]
    public async Task GetSharedConsentDraftAsync_WhenSharedConsentDoesNotExist_ReturnsNull()
    {
        using var repository = ShareCodeRepository();

        var foundConsent = await repository.GetSharedConsentDraftAsync(Guid.NewGuid(), Guid.NewGuid());

        foundConsent.Should().BeNull();
    }

    [Fact]
    public async Task ShareCodeDocumentExistsAsync_WhenDoesNotExist_ReturnsFalse()
    {
        using var repository = ShareCodeRepository();

        var exists = await repository.ShareCodeDocumentExistsAsync("share_code", "document_name");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ShareCodeDocumentExistsAsync_WhenExists_ReturnsTrue()
    {
        var sharedConsent = GivenSharedConsent();

        var section = new FormSection
        {
            Form = sharedConsent.Form,
            FormId = sharedConsent.FormId,
            Title = "form_section",
            Type = FormSectionType.Standard,
            DisplayOrder = 0,
            AllowsMultipleAnswerSets = true,
            CheckFurtherQuestionsExempted = true,
            Configuration = new(),
            Guid = Guid.NewGuid(),
            Questions = []
        };

        var question = new FormQuestion
        {
            Guid = Guid.NewGuid(),
            NextQuestion = null,
            NextQuestionAlternative = null,
            SortOrder = 0,
            Section = section,
            Type = FormQuestionType.FileUpload,
            IsRequired = true,
            Name = "question_1",
            Title = "title_1",
            Description = null,
            Caption = null,
            Options = new FormQuestionOptions()
        };

        section.Questions.Add(question);

        var answerSet = new FormAnswerSet
        {
            Guid = Guid.NewGuid(),
            SharedConsentId = sharedConsent.Id,
            SharedConsent = sharedConsent,
            FurtherQuestionsExempted = false,
            Section = section,
            SectionId = section.Id,
            Answers = []
        };

        answerSet.Answers.Add(new FormAnswer
        {
            Guid = Guid.NewGuid(),
            FormAnswerSetId = answerSet.Id,
            Question = question,
            QuestionId = question.Id,
            TextValue = "document_name"
        });

        sharedConsent.ShareCode = "share_code";
        sharedConsent.AnswerSets.Add(answerSet);

        await using var context = GetDbContext();
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = ShareCodeRepository();

        var exists = await repository.ShareCodeDocumentExistsAsync("share_code", "document_name");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetSharedConsentDraftAsync_WhenItDoesExist_ReturnsIt()
    {
        var sharedConsent = GivenSharedConsent();

        await using var context = GetDbContext();
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = ShareCodeRepository();

        var found = await repository.GetSharedConsentDraftAsync(sharedConsent.Form.Guid, sharedConsent.Organisation.Guid);

        found.Should().NotBeNull();
        found.As<SharedConsent>().OrganisationId.Should().Be(sharedConsent.OrganisationId);
        found.As<SharedConsent>().SubmissionState.Should().Be(SubmissionState.Draft);
        found.As<SharedConsent>().ShareCode.Should().BeNull();
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

        sharedConsent.ShareCode = bookingref;
        sharedConsent.SubmissionState = SubmissionState.Submitted;
        sharedConsent.SubmittedAt = DateTime.UtcNow;

        await using var context = GetDbContext();
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = ShareCodeRepository();

        var found = await repository.GetShareCodesAsync(sharedConsent.Organisation.Guid);

        var sharedConsents = found.ToList();
        sharedConsents.Should().NotBeEmpty();
        sharedConsents.Should().HaveCount(1);

        sharedConsents.First().As<SharedConsent>().OrganisationId.Should().Be(sharedConsent.OrganisationId);
        sharedConsents.First().As<SharedConsent>().SubmissionState.Should().Be(SubmissionState.Submitted);
        sharedConsents.First().As<SharedConsent>().ShareCode.Should().Be(bookingref);
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
        GivenAnswer(question, answerSet);

        sharedConsent.SubmissionState = SubmissionState.Submitted;
        sharedConsent.SubmittedAt = DateTime.UtcNow;
        var shareCode = "EXISTING-CODE-2";
        sharedConsent.ShareCode = shareCode;

        await using var context = GetDbContext();
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        var conn = (NpgsqlConnection)context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();
        using var command = new NpgsqlCommand("CALL create_shared_consent_snapshot(@p_share_code)", conn);
        command.Parameters.AddWithValue("p_share_code", shareCode);
        await command.ExecuteNonQueryAsync();
        await conn.CloseAsync();

        using var repository = ShareCodeRepository();

        var foundConsent = await repository.GetByShareCode(shareCode);

        foundConsent.Should().NotBeNull();
        foundConsent!.ShareCode.Should().Be(shareCode);
        foundConsent.Organisation.Name.Should().StartWith("New Corporation");
        foundConsent.Organisation.Identifiers.Should().ContainSingle(i => i.LegalName == "New Corporation Ltd");
        foundConsent.Organisation.ContactPoints.Should().ContainSingle(cp => cp.Name == "Procurement Team");

        var registeredAddress = foundConsent.Organisation.Addresses.FirstOrDefault(a => a.Type == AddressType.Registered);
        registeredAddress.Should().NotBeNull();
        registeredAddress.As<AddressNonEf>().Should().NotBeNull();
        registeredAddress.As<AddressNonEf>().StreetAddress.Should().Be("82 St. John’s Road");
        registeredAddress.As<AddressNonEf>().PostalCode.Should().Be("CH43 7UR");

        foundConsent.AnswerSets.Should().ContainSingle();

        var retrievedAnswerSet = foundConsent.AnswerSets.First();
        retrievedAnswerSet.Answers.Should().ContainSingle(a => a.BoolValue == true && a.Question.Title == "Yes or no?");

        var retrievedForm = foundConsent.Form;
        retrievedForm.Should().NotBeNull();
        retrievedForm.Name.Should().Be("Test Form");
    }


    [Fact]
    public async Task GetShareCodeVerifyAsync_WhenShareCodeIsLatest_ReturnsTrue()
    {
        var form = GivenForm(Guid.NewGuid());
        var section = GivenSection(Guid.NewGuid(), form);
        var question = GivenYesOrNoQuestion(section);
        var sharedConsent = GivenSharedConsent(form);
        var answerSet = GivenAnswerSet(sharedConsent, section);
        GivenAnswer(question, answerSet);

        sharedConsent.SubmissionState = SubmissionState.Submitted;
        sharedConsent.SubmittedAt = DateTime.UtcNow;

        var shareCode = "EXISTING-CODE-1";

        sharedConsent.ShareCode = shareCode;

        await using var context = GetDbContext();
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = ShareCodeRepository();

        var foundConsent = await repository.GetShareCodeVerifyAsync(form.Version, shareCode);

        foundConsent.Should().NotBeNull();
        foundConsent.Should().Be(true);
    }

    [Fact]
    public async Task GetConsortiumOrganisationsShareCode_WhenConsortium_ReturnsChildOrganisationsShareCodeList()
    {
        var consortiumSharedConsent = GivenSharedConsent();
        consortiumSharedConsent.ShareCode = "consortium-sharecode";
        consortiumSharedConsent.SubmissionState = SubmissionState.Submitted;
        consortiumSharedConsent.SubmittedAt = DateTime.UtcNow;

        var consortiumOrgSharedConsent = GivenSharedConsent();
        consortiumOrgSharedConsent.ShareCode = "child-sharecode";
        consortiumOrgSharedConsent.SubmissionState = SubmissionState.Submitted;
        consortiumOrgSharedConsent.SubmittedAt = DateTime.UtcNow;

        await using var context = GetDbContext();
        await context.SharedConsents.AddAsync(consortiumSharedConsent);
        await context.SharedConsents.AddAsync(consortiumOrgSharedConsent);
        await context.SharedConsentConsortiums.AddAsync(new SharedConsentConsortium
        {
            ParentSharedConsentId = consortiumSharedConsent.Id,
            ParentSharedConsent = consortiumSharedConsent,
            ChildSharedConsentId = consortiumOrgSharedConsent.Id,
            ChildSharedConsent = consortiumOrgSharedConsent
        });
        await context.SaveChangesAsync();

        using var repository = ShareCodeRepository();
        var shareCodeList = await repository.GetConsortiumOrganisationsShareCode("consortium-sharecode");

        shareCodeList.Should().HaveCount(1);
        shareCodeList.First().As<string>().Should().Be("child-sharecode");
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
            FormVersionId = form.Version,
            ShareCode = null
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
            CheckFurtherQuestionsExempted = false,
            DisplayOrder = 1,
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

    private static void GivenAnswer(FormQuestion question, FormAnswerSet answerSet)
    {
        var answer = new FormAnswer
        {
            Guid = Guid.NewGuid(),
            QuestionId = question.Id,
            Question = question,
            FormAnswerSetId = answerSet.Id,
            FormAnswerSet = answerSet,
            BoolValue = true
        };
        answerSet.Answers.Add(answer);
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
            FurtherQuestionsExempted = false
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
            Name = "_Section0" + GetQuestionNumber(),
            Title = "Yes or no?",
            Description = "Please answer.",
            NextQuestion = null,
            NextQuestionAlternative = null,
            Caption = null,
            Options = new FormQuestionOptions
            {
                Choices = null,
                Groups = null
            },
            SortOrder = 0
        };
        section.Questions.Add(question);
        return question;
    }

    private static Organisation GivenOrganisation()
    {
        return new Organisation
        {
            Guid = Guid.NewGuid(),
            Name = "New Corporation " + Guid.NewGuid().ToString(),
            Type = OrganisationType.Organisation,
            Tenant = GivenTenant(),
            Identifiers = new List<Persistence.Identifier>
        {
            new Persistence.Identifier
            {
                IdentifierId = Guid.NewGuid().ToString(),
                Scheme = "GB-PPON",
                LegalName = "New Corporation Ltd",
                Uri = "https://cdp.cabinetoffice.gov.uk/organisations/" + Guid.NewGuid().ToString(),
                Primary = true
            }
        },
            ContactPoints = new List<Persistence.ContactPoint>
        {
            new Persistence.ContactPoint
            {
                Name = "Procurement Team",
                Email = "info@example.com",
                Telephone = "+441234567890"
            }
        },
            Addresses = new List<Persistence.OrganisationAddress>
        {
            new()
            {
                Type = AddressType.Registered,
                Address = new Address
                {
                    StreetAddress = "82 St. John’s Road",
                    Locality = "CHESTER",
                    Region = "Lancashire",
                    PostalCode = "CH43 7UR",
                    CountryName = "United Kingdom",
                    Country = "GB"
                }
            }
        }
        };
    }

    private DatabaseShareCodeRepository ShareCodeRepository()
        => new(GetDbContext());

    private OrganisationInformationContext? context = null;

    private OrganisationInformationContext GetDbContext()
    {
        context = context ?? postgreSql.OrganisationInformationContext();
        return context;
    }
}