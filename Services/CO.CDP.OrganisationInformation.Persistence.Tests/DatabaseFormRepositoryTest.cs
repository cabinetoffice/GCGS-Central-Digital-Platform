using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseFormRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task GetFormSummaryAsync_WhenFormDoesNotExists_ReturnsEmptyCollection()
    {
        using var repository = FormRepository();

        var summaries = await repository.GetFormSummaryAsync(Guid.NewGuid(), Guid.NewGuid());

        summaries.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetFormSummaryAsync_WhenAnswerSetNotExists_ReturnsAnswerSetCountAsZero()
    {
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var form = GivenForm(formId: formId);
        var sharedConsent = GivenSharedConsent(form);
        var section = GivenSection(sectionId, form);

        await using var context = postgreSql.OrganisationInformationContext();
        await context.Forms.AddAsync(form);
        await context.Set<FormSection>().AddAsync(section);
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = FormRepository();

        var summaries = await repository.GetFormSummaryAsync(formId, sharedConsent.Organisation.Guid);

        summaries.Should().HaveCount(1);
        summaries.First().SectionId.Should().Be(sectionId);
        summaries.First().AnswerSetCount.Should().Be(0);
    }

    [Fact]
    public async Task GetFormSummaryAsync_WhenAnswerSetExists_ReturnsCorrectAnswerSetCount()
    {
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var form = GivenForm(formId: formId);
        var sharedConsent = GivenSharedConsent(form);
        var section = GivenSection(sectionId, form);
        GivenAnswerSet(sharedConsent: sharedConsent, section: section);

        await using var context = postgreSql.OrganisationInformationContext();
        await context.Forms.AddAsync(form);
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = FormRepository();

        var summaries = await repository.GetFormSummaryAsync(formId, sharedConsent.Organisation.Guid);

        summaries.Should().HaveCount(1);
        summaries.First().SectionId.Should().Be(sectionId);
        summaries.First().AnswerSetCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSectionAsync_WhenSectionDoesNotExist_ReturnsNull()
    {
        using var repository = FormRepository();

        var foundSection = await repository.GetSectionAsync(Guid.NewGuid(), Guid.NewGuid());

        foundSection.Should().BeNull();
    }

    [Fact]
    public async Task GetQuestionsAsync_WhenSectionDoesNotExist_ReturnsEmptyList()
    {
        using var repository = FormRepository();

        var nonExistentSectionId = Guid.NewGuid();

        var foundQuestions = await repository.GetQuestionsAsync(nonExistentSectionId);

        foundQuestions.Should().BeEmpty();
    }

    [Fact]
    public async Task GetQuestionsAsync_WhenSectionExists_ReturnsQuestions()
    {
        using var repository = FormRepository();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var form = GivenForm(formId);
        var section = GivenSection(sectionId, form);
        form.Sections.Add(section);

        var question1 = new FormQuestion
        {
            Guid = Guid.NewGuid(),
            Section = section,
            Title = "Question 1",
            Caption = "Question Caption",
            Description = "Question 1 desc",
            Type = FormQuestionType.Text,
            IsRequired = true,
            NextQuestion = null,
            NextQuestionAlternative = null,
            Options = new FormQuestionOptions()
        };

        var question2 = new FormQuestion
        {
            Guid = Guid.NewGuid(),
            Section = section,
            Title = "Question 2",
            Caption = "Question Caption",
            Description = "Question 2 desc",
            Type = FormQuestionType.YesOrNo,
            IsRequired = true,
            NextQuestion = null,
            NextQuestionAlternative = null,
            Options = new FormQuestionOptions()
        };

        section.Questions.Add(question1);
        section.Questions.Add(question2);

        await repository.SaveFormAsync(form);

        var foundQuestions = await repository.GetQuestionsAsync(sectionId);

        foundQuestions.Should().NotBeEmpty();
        foundQuestions.Should().HaveCount(2);
        foundQuestions.Should().ContainEquivalentOf(question1, config => config
            .Excluding(ctx => ctx.Id)
            .Excluding(ctx => ctx.CreatedOn)
            .Excluding(ctx => ctx.UpdatedOn)
        );
        foundQuestions.Should().ContainEquivalentOf(question2, config => config
            .Excluding(ctx => ctx.Id)
            .Excluding(ctx => ctx.CreatedOn)
            .Excluding(ctx => ctx.UpdatedOn)
        );
    }

    [Fact]
    public async Task GetSectionAsync_WhenSectionExist_ReturnsFormSection()
    {
        using var repository = FormRepository();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var form = GivenForm(formId);
        var section = GivenSection(sectionId, form);
        form.Sections.Add(section);
        await repository.SaveFormAsync(form);

        var foundSection = await repository.GetSectionAsync(formId, sectionId);

        foundSection.Should().NotBeNull();
        foundSection?.Form.Should().NotBeNull();
        foundSection.Should().BeEquivalentTo(section, config => config
            .Excluding(ctx => ctx.Id)
            .Excluding(ctx => ctx.CreatedOn)
            .Excluding(ctx => ctx.UpdatedOn)
        );
    }

    [Fact]
    public async Task GetFormAnswerSetsAsync_WhenFormAnswerSetDoesNotExist_ReturnsNull()
    {
        using var repository = FormRepository();

        var nonExistentSectionId = Guid.NewGuid();
        var nonExistentOrganisationId = Guid.NewGuid();

        var foundAnswerSets = await repository.GetFormAnswerSetsAsync(nonExistentSectionId, nonExistentOrganisationId);

        foundAnswerSets.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSharedConsentDraftAsync_WhenSharedConsentDoesNotExist_ReturnsNull()
    {
        using var repository = FormRepository();

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

        using var repository = FormRepository();

        var found = await repository.GetSharedConsentDraftAsync(sharedConsent.Form.Guid, sharedConsent.Organisation.Guid);

        found.Should().NotBeNull();
        found.As<SharedConsent>().OrganisationId.Should().Be(sharedConsent.OrganisationId);
        found.As<SharedConsent>().SubmissionState.Should().Be(SubmissionState.Draft);
        found.As<SharedConsent>().BookingReference.Should().BeNull();
    }

    [Fact]
    public async Task GetSharedConsentDraftWithAnswersAsync_WhenSharedConsentDoesNotExist_ReturnsNull()
    {
        using var repository = FormRepository();

        var foundConsent = await repository.GetSharedConsentDraftWithAnswersAsync(Guid.NewGuid(), Guid.NewGuid());

        foundConsent.Should().BeNull();
    }

    [Fact]
    public async Task GetSharedConsentDraftWithAnswersAsync_WhenSharedConsentDoesExist_ReturnsIt()
    {
        var sharedConsent = GivenSharedConsent();

        await using var context = postgreSql.OrganisationInformationContext();
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = FormRepository();

        var found = await repository.GetSharedConsentDraftWithAnswersAsync(sharedConsent.Form.Guid, sharedConsent.Organisation.Guid);

        found.Should().NotBeNull();
        found.As<SharedConsent>().OrganisationId.Should().Be(sharedConsent.OrganisationId);
        found.As<SharedConsent>().SubmissionState.Should().Be(SubmissionState.Draft);
        found.As<SharedConsent>().BookingReference.Should().BeNull();
    }

    [Fact]
    public async Task GetSharedConsentDraftWithAnswersAsync_WhenSharedConsentWithAnswersExists_ReturnsIt()
    {
        var formId = Guid.NewGuid();

        var form = GivenForm(formId: formId);
        var sharedConsent = GivenSharedConsent(form);
        var section = GivenSection(sectionId: Guid.NewGuid(), form: form);
        var question = GivenYesOrNoQuestion(section: section);
        var answerSet = GivenAnswerSet(sharedConsent: sharedConsent, section: section);
        var answer = GivenAnswer(question: question, answerSet: answerSet);

        await using var context = postgreSql.OrganisationInformationContext();
        await context.Forms.AddAsync(form);
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = FormRepository();

        var found = await repository.GetSharedConsentDraftWithAnswersAsync(sharedConsent.Form.Guid, sharedConsent.Organisation.Guid);

        found.Should().NotBeNull();
        found.As<SharedConsent>().OrganisationId.Should().Be(sharedConsent.OrganisationId);
        found.As<SharedConsent>().SubmissionState.Should().Be(SubmissionState.Draft);
        found.As<SharedConsent>().AnswerSets.Should().NotBeEmpty();
        found.As<SharedConsent>().AnswerSets.First().Guid.Should().Be(answerSet.Guid);
        found.As<SharedConsent>().AnswerSets.First().Answers.Should().NotBeEmpty();
        found.As<SharedConsent>().AnswerSets.First().Answers.First().Guid.Should().Be(answer.Guid);
    }

    [Fact]
    public async Task GetShareCodesAsync_WhenCodesDoesNotExist_ReturnsEmptyList()
    {
        using var repository = FormRepository();

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

        using var repository = FormRepository();

        var found = await repository.GetShareCodesAsync(sharedConsent.Organisation.Guid);

        found.Should().NotBeEmpty();
        found.Should().HaveCount(1);

        found.First().As<SharedConsent>().OrganisationId.Should().Be(sharedConsent.OrganisationId);
        found.First().As<SharedConsent>().SubmissionState.Should().Be(SubmissionState.Submitted);
        found.First().As<SharedConsent>().BookingReference.Should().Be(bookingref);
    }

    [Fact]
    public async Task DeleteAnswerSetAsync_ShouldReturnFalse_WhenAnswerSetNotFound()
    {
        var organisationId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();
        var repository = FormRepository();

        var result = await repository.DeleteAnswerSetAsync(organisationId, answerSetId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetFormAnswerSetsAsync_WhenFormAnswerSetsDoNotExist_ReturnsEmptyList()
    {
        using var repository = FormRepository();

        var nonExistentSectionId = Guid.NewGuid();
        var nonExistentOrganisationId = Guid.NewGuid();

        var foundAnswerSets = await repository.GetFormAnswerSetsAsync(nonExistentSectionId, nonExistentOrganisationId);

        foundAnswerSets.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFormAnswerSetAsync_WhenFormAnswerSetDoesNotExist_ReturnsNull()
    {
        using var repository = FormRepository();

        var nonExistentAnswerSetId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        var foundAnswerSet = await repository.GetFormAnswerSetsAsync(nonExistentAnswerSetId, organisationId);

        foundAnswerSet.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetFormAnswerSetAsync_WhenFormAnswerSetsExists_ReturnsFormAnswerSet()
    {
        using var context = postgreSql.OrganisationInformationContext();
        var repository = new DatabaseFormRepository(context);

        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var form = GivenForm(formId);
        var section = GivenSection(sectionId, form);
        form.Sections.Add(section);
        var question = new FormQuestion
        {
            Guid = questionId,
            Section = section,
            Title = "Question 1",
            Caption = "Question Caption",
            Description = "Question 1 desc",
            Type = FormQuestionType.YesOrNo,
            IsRequired = true,
            Options = new FormQuestionOptions(),
            NextQuestion = null,
            NextQuestionAlternative = null,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
        section.Questions.Add(question);

        context.Forms.Add(form);
        await context.SaveChangesAsync();

        var organisation = GivenOrganisation(organisationId);
        context.Organisations.Add(organisation);
        await context.SaveChangesAsync();

        var sharedConsent = GivenSharedConsent(form, null, organisation);
        var answerSet = new FormAnswerSet
        {
            Guid = Guid.NewGuid(),
            SharedConsentId = sharedConsent.Id,
            SharedConsent = sharedConsent,
            SectionId = section.Id,
            Section = section,
            Answers = new List<FormAnswer>(),
            Deleted = false,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        var formAnswer = new FormAnswer
        {
            Guid = Guid.NewGuid(),
            QuestionId = question.Id,
            Question = question,
            FormAnswerSetId = answerSet.Id,
            FormAnswerSet = answerSet,
            BoolValue = true,
            AddressValue = new FormAddress { StreetAddress = "456 Elm St", Locality = "London", PostalCode = "G67890", CountryName = "UK", Country = "GB" },
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        answerSet.Answers.Add(formAnswer);

        context.FormAnswerSets.Add(answerSet);
        await context.SaveChangesAsync();

        var foundAnswerSet = await repository.GetFormAnswerSetsAsync(sectionId, organisation.Guid);

        foundAnswerSet.Should().NotBeNull();
        foundAnswerSet!.Should().HaveCount(1);
        foundAnswerSet[0].Answers.Should().HaveCount(1);
        var foundAnswer = foundAnswerSet[0].Answers.First();
        foundAnswer.Question.Should().Be(question);
        foundAnswer.BoolValue.Should().BeTrue();
    }

    [Fact]
    public async Task GetFormAnswerSetAsync_WhenFormAnswerSetDeleted_DoesNotReturnsFormAnswerSet()
    {
        using var context = postgreSql.OrganisationInformationContext();
        var repository = new DatabaseFormRepository(context);

        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var form = GivenForm(formId);
        var section = GivenSection(sectionId, form);
        form.Sections.Add(section);

        context.Forms.Add(form);
        await context.SaveChangesAsync();

        var organisation = GivenOrganisation(organisationId);
        context.Organisations.Add(organisation);
        await context.SaveChangesAsync();

        var sharedConsent = GivenSharedConsent(form);
        var answerSet = new FormAnswerSet
        {
            Guid = answerSetId,
            SharedConsentId = sharedConsent.Id,
            SharedConsent = sharedConsent,
            SectionId = section.Id,
            Section = section,
            Answers = [],
            Deleted = true,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        context.FormAnswerSets.Add(answerSet);
        await context.SaveChangesAsync();

        var foundAnswerSet = await repository.GetFormAnswerSetsAsync(sectionId, organisation.Guid);

        foundAnswerSet.Should().NotBeNull();
        foundAnswerSet!.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetFormAnswerSetAsync_WhenFormAnswerSetExists_ReturnsFormAnswerSet()
    {
        using var context = postgreSql.OrganisationInformationContext();
        var repository = new DatabaseFormRepository(context);

        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();

        var form = GivenForm(formId);
        var section = GivenSection(sectionId, form);
        form.Sections.Add(section);

        context.Forms.Add(form);
        await context.SaveChangesAsync();

        var organisation = GivenOrganisation(organisationId);
        context.Organisations.Add(organisation);
        await context.SaveChangesAsync();

        var sharedConsent = GivenSharedConsent(form, null, organisation);
        var answerSet = new FormAnswerSet
        {
            Guid = answerSetId,
            SharedConsentId = sharedConsent.Id,
            SharedConsent = sharedConsent,
            SectionId = section.Id,
            Section = section,
            Answers = new List<FormAnswer>(),
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        context.FormAnswerSets.Add(answerSet);
        await context.SaveChangesAsync();

        var foundAnswerSet = await repository.GetFormAnswerSetAsync(sectionId, organisation.Guid, answerSetId);

        foundAnswerSet.Should().NotBeNull();
        foundAnswerSet.Should().BeEquivalentTo(answerSet, config => config
            .Excluding(ctx => ctx.Id)
            .Excluding(ctx => ctx.CreatedOn)
            .Excluding(ctx => ctx.UpdatedOn));
    }

    [Fact]
    public async Task SaveAnswerSet_ShouldSaveNewAnswerSet()
    {
        using var repository = FormRepository();
        using var orgRepository = OrganisationRepository();

        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var guid = Guid.NewGuid();
        var organisation = GivenOrganisation(guid: guid, name: "Organisation1");

        var form = GivenForm(formId);
        var section = GivenSection(sectionId, form);
        form.Sections.Add(section);
        var question = new FormQuestion
        {
            Guid = questionId,
            Section = section,
            Title = "Question 1",
            Caption = "Question Caption",
            Description = "Question 1 desc",
            Type = FormQuestionType.YesOrNo,
            IsRequired = true,
            Options = new FormQuestionOptions(),
            NextQuestion = null,
            NextQuestionAlternative = null,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
        section.Questions.Add(question);
        await repository.SaveFormAsync(form);

        var sharedConsent = GivenSharedConsent(form, null, organisation);
        var answerSet = new FormAnswerSet
        {
            Guid = Guid.NewGuid(),
            SharedConsentId = sharedConsent.Id,
            SharedConsent = sharedConsent,
            SectionId = section.Id,
            Section = section,
            Answers = new List<FormAnswer>(),
            Deleted = false,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        var formAnswer = new FormAnswer
        {
            Guid = Guid.NewGuid(),
            QuestionId = question.Id,
            Question = question,
            FormAnswerSetId = answerSet.Id,
            FormAnswerSet = answerSet,
            BoolValue = true,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        answerSet.Answers.Add(formAnswer);

        await repository.SaveAnswerSet(answerSet);

        var foundAnswerSet = await repository.GetFormAnswerSetAsync(section.Guid, organisation.Guid, answerSet.Guid);

        foundAnswerSet.Should().NotBeNull();
        foundAnswerSet!.Answers.Should().HaveCount(1);
        var foundAnswer = foundAnswerSet.Answers.First();
        foundAnswer.Question.Should().Be(question);
        foundAnswer.BoolValue.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateFormAnswerSet_ShouldUpdateExistingAnswerSet()
    {
        using var repository = FormRepository();
        using var orgRepository = OrganisationRepository();

        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var guid = Guid.NewGuid();
        var organisation = GivenOrganisation(guid: guid, name: "Organisation2");

        var form = GivenForm(formId);
        var section = GivenSection(sectionId, form);
        form.Sections.Add(section);
        var question = new FormQuestion
        {
            Guid = questionId,
            Section = section,
            Title = "Question 1",
            Caption = "Question Caption",
            Description = "Question 1 desc",
            Type = FormQuestionType.YesOrNo,
            IsRequired = true,
            Options = new FormQuestionOptions(),
            NextQuestion = null,
            NextQuestionAlternative = null,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
        section.Questions.Add(question);
        await repository.SaveFormAsync(form);

        var sharedConsnt = new SharedConsent
        {
            Guid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            FormId = form.Id,
            Form = form,
            AnswerSets = new List<FormAnswerSet>(),
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = DateTimeOffset.UtcNow,
            FormVersionId = "1.0",
            BookingReference = string.Empty,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        var answerSet = new FormAnswerSet
        {
            Guid = Guid.NewGuid(),
            SharedConsentId = sharedConsnt.Id,
            SharedConsent = sharedConsnt,
            SectionId = section.Id,
            Section = section,
            Answers = new List<FormAnswer>(),
            Deleted = false,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        var formAnswer = new FormAnswer
        {
            Guid = Guid.NewGuid(),
            QuestionId = question.Id,
            Question = question,
            FormAnswerSetId = answerSet.Id,
            FormAnswerSet = answerSet,
            BoolValue = true,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        answerSet.Answers.Add(formAnswer);

        var newAnswer = new FormAnswer
        {
            Guid = Guid.NewGuid(),
            QuestionId = question.Id,
            Question = question,
            FormAnswerSetId = answerSet.Id,
            FormAnswerSet = answerSet,
            BoolValue = false,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
        answerSet.Answers.Clear();
        answerSet.Answers.Add(newAnswer);
        await repository.SaveAnswerSet(answerSet);

        var foundAnswerSet = await repository.GetFormAnswerSetAsync(section.Guid, organisation.Guid, answerSet.Guid);

        foundAnswerSet.Should().NotBeNull();
        foundAnswerSet!.Answers.Should().HaveCount(1);
        var foundAnswer = foundAnswerSet.Answers.First();
        foundAnswer.Question.Should().Be(question);
        foundAnswer.BoolValue.Should().BeFalse();
    }

    [Fact]
    public async Task GetQuestionsAsync_WhenOptionsAreSimple_ReturnsCorrectOptions()
    {
        using var repository = FormRepository();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var form = GivenForm(formId);
        var section = GivenSection(sectionId, form);
        form.Sections.Add(section);

        var questionId = Guid.NewGuid();
        var choiceId = Guid.NewGuid();

        var simpleOptions = new FormQuestionOptions
        {
            Choices = new List<FormQuestionChoice>
        {
            new FormQuestionChoice
            {
                Id = choiceId,
                Title = "Choice 1",
                GroupName = "Group 1",
                Hint = new FormQuestionChoiceHint
                {
                    Title = "Hint Title",
                    Description = "Hint Description"
                }
            }
        },
            ChoiceProviderStrategy = "SimpleStrategy"
        };

        var question = new FormQuestion
        {
            Guid = questionId,
            Section = section,
            Title = "Question with Simple Options",
            Caption = "Question Caption",
            Description = "This is a test question with simple options.",
            Type = FormQuestionType.SingleChoice,
            IsRequired = true,
            NextQuestion = null,
            NextQuestionAlternative = null,
            Options = simpleOptions
        };

        section.Questions.Add(question);
        await repository.SaveFormAsync(form);

        var foundQuestions = await repository.GetQuestionsAsync(sectionId);

        foundQuestions.Should().NotBeEmpty();
        foundQuestions.Should().HaveCount(1);

        var foundQuestion = foundQuestions.First();
        foundQuestion.Options.Should().NotBeNull();

        foundQuestion.Options!.Choices.Should().NotBeNull().And.HaveCount(1);

        var foundChoice = foundQuestion?.Options?.Choices?.First();
        foundChoice.Should().BeEquivalentTo(simpleOptions.Choices.First(), config => config.Excluding(ctx => ctx.Id));
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

    private static SharedConsent GivenSharedConsent(
        Form? form = null,
        List<FormAnswerSet>? answerSets = null,
        Organisation? organisation = null
        )
    {
        form ??= GivenForm(Guid.NewGuid());
        organisation ??= GivenOrganisation();

        return new SharedConsent()
        {
            Guid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            FormId = form.Id,
            Form = form,
            AnswerSets = answerSets ?? [],
            SubmissionState = SubmissionState.Draft,
            SubmittedAt = null,
            FormVersionId = "202404",
            BookingReference = null
        };
    }

    private static FormAnswer GivenAnswer(FormQuestion question, FormAnswerSet answerSet)
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

    private IFormRepository FormRepository()
    {
        return new DatabaseFormRepository(postgreSql.OrganisationInformationContext());
    }

    private IOrganisationRepository OrganisationRepository()
    {
        return new DatabaseOrganisationRepository(postgreSql.OrganisationInformationContext());
    }
}