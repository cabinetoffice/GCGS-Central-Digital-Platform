using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static CO.CDP.OrganisationInformation.Persistence.Tests.Factories.SharedConsentFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseFormRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    private static int _nextQuestionNumber = 100;

    private static int GetQuestionNumber()
    {
        Interlocked.Increment(ref _nextQuestionNumber);

        return _nextQuestionNumber;
    }

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
        var sharedConsent = GivenSharedConsent(form: form);
        var section = GivenFormSection(sectionId, form);

        await using var context = postgreSql.OrganisationInformationContext();
        await context.Forms.AddAsync(form);
        await context.Set<FormSection>().AddAsync(section);
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = FormRepository(context);

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
        var sharedConsent = GivenSharedConsent(form: form);
        var section = GivenFormSection(sectionId, form);
        GivenAnswerSet(sharedConsent: sharedConsent, section: section);

        await using var context = postgreSql.OrganisationInformationContext();
        await context.Forms.AddAsync(form);
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = FormRepository(context);

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
        await using var context = postgreSql.OrganisationInformationContext();
        using var repository = FormRepository(context);
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var form = GivenForm(formId);
        var section = GivenFormSection(sectionId, form);
        form.Sections.Add(section);

        var question1 = new FormQuestion
        {
            Guid = Guid.NewGuid(),
            Section = section,
            Name = "_Section0" + GetQuestionNumber(),
            Title = "Question 1",
            Caption = "Question Caption",
            Description = "Question 1 desc",
            Type = FormQuestionType.Text,
            IsRequired = true,
            NextQuestion = null,
            NextQuestionAlternative = null,
            Options = new FormQuestionOptions(),
            SortOrder = 0
        };

        var question2 = new FormQuestion
        {
            Guid = Guid.NewGuid(),
            Section = section,
            Name = "_Section0" + GetQuestionNumber(),
            Title = "Question 2",
            Caption = "Question Caption",
            Description = "Question 2 desc",
            Type = FormQuestionType.YesOrNo,
            IsRequired = true,
            NextQuestion = null,
            NextQuestionAlternative = null,
            Options = new FormQuestionOptions(),
            SortOrder = 0
        };

        section.Questions.Add(question1);
        section.Questions.Add(question2);

        await context.Forms.AddAsync(form);
        await context.SaveChangesAsync();

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
        await using var context = postgreSql.OrganisationInformationContext();
        using var repository = FormRepository(context);
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var form = GivenForm(formId);
        var section = GivenFormSection(sectionId, form);
        form.Sections.Add(section);
        await context.Forms.AddAsync(form);
        await context.SaveChangesAsync();

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

        var foundAnswerSets =
            await repository.GetFormAnswerSetsFromCurrentSharedConsentAsync(nonExistentSectionId,
                nonExistentOrganisationId);

        foundAnswerSets.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSharedConsentWithAnswersAsync_WhenSharedConsentDoesNotExist_ReturnsNull()
    {
        using var repository = FormRepository();

        var foundConsent = await repository.GetSharedConsentWithAnswersAsync(Guid.NewGuid(), Guid.NewGuid());

        foundConsent.Should().BeNull();
    }

    [Fact]
    public async Task GetSharedConsentWithAnswersAsync_WhenSharedConsentDoesExist_ReturnsIt()
    {
        var sharedConsent = GivenSharedConsent();

        await using var context = postgreSql.OrganisationInformationContext();
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = FormRepository(context);

        var found = await repository.GetSharedConsentWithAnswersAsync(sharedConsent.Form.Guid,
            sharedConsent.Organisation.Guid);

        found.Should().NotBeNull();
        found.As<SharedConsent>().OrganisationId.Should().Be(sharedConsent.OrganisationId);
        found.As<SharedConsent>().ShareCode.Should().BeNull();
    }

    [Fact]
    public async Task GetSharedConsentWithAnswersAsync_WhenSharedConsentWithAnswersExists_ReturnsIt()
    {
        var formId = Guid.NewGuid();

        var form = GivenForm(formId: formId);
        var sharedConsent = GivenSharedConsent(form: form);
        var section = GivenFormSection(sectionId: Guid.NewGuid(), form: form);
        var question = GivenYesOrNoQuestion(section: section);
        var answerSet = GivenAnswerSet(sharedConsent: sharedConsent, section: section);
        var answer = GivenAnswer(question: question, answerSet: answerSet);

        await using var context = postgreSql.OrganisationInformationContext();
        await context.Forms.AddAsync(form);
        await context.SharedConsents.AddAsync(sharedConsent);
        await context.SaveChangesAsync();

        using var repository = FormRepository(context);

        var found = await repository.GetSharedConsentWithAnswersAsync(sharedConsent.Form.Guid,
            sharedConsent.Organisation.Guid);

        found.Should().NotBeNull();
        found.As<SharedConsent>().OrganisationId.Should().Be(sharedConsent.OrganisationId);
        found.As<SharedConsent>().AnswerSets.Should().NotBeEmpty();
        found.As<SharedConsent>().AnswerSets.First().Guid.Should().Be(answerSet.Guid);
        found.As<SharedConsent>().AnswerSets.First().Answers.Should().NotBeEmpty();
        found.As<SharedConsent>().AnswerSets.First().Answers.First().Guid.Should().Be(answer.Guid);
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
    public async Task DeleteAnswerSetAsync_ShouldDeleteExistingAnswerSet_WhenAnswerSetFoundForDraftSharedConsent()
    {
        await using var context = postgreSql.OrganisationInformationContext();
        var organisationId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();
        var organisation = GivenOrganisation(organisationId: organisationId);
        var form = GivenForm();
        var section = GivenFormSection(form: form);
        var sharedConsent = GivenSharedConsent(
            state: SubmissionState.Draft,
            organisation: organisation,
            form: form);
        var answerSet = GivenAnswerSet(sharedConsent: sharedConsent, answerSetId: answerSetId, section: section);
        GivenAnswer(
            question: GivenFormQuestion(type: FormQuestionType.Text, section: section),
            textValue: "Answer 1",
            answerSet: answerSet);

        context.Organisations.Add(organisation);
        context.SharedConsents.Add(sharedConsent);
        await context.SaveChangesAsync();

        var repository = FormRepository(context);

        var result = await repository.DeleteAnswerSetAsync(organisationId, answerSetId);
        var foundAnswerSet = await context.FormAnswerSets.FirstAsync(a => a.Guid == answerSetId);
        var sharedConsents = context.SharedConsents
            .Where(s => s.OrganisationId == organisation.Id)
            .ToList();

        result.Should().BeTrue();
        foundAnswerSet.As<FormAnswerSet>().Deleted.Should().BeTrue();
        sharedConsents.Count.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAnswerSetAsync_ShouldDeleteClonedAnswerSet_WhenAnswerSetFoundForSubmittedSharedConsent()
    {
        await using var context = postgreSql.OrganisationInformationContext();
        var organisationId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();
        var organisation = GivenOrganisation(organisationId: organisationId);
        var form = GivenForm();
        var section = GivenFormSection(form: form);
        var sharedConsent = GivenSharedConsent(
            state: SubmissionState.Submitted,
            organisation: organisation,
            form: form);
        var answerSet = GivenAnswerSet(sharedConsent: sharedConsent, answerSetId: answerSetId, section: section);
        GivenAnswer(
            question: GivenFormQuestion(type: FormQuestionType.Text, section: section),
            textValue: "Answer 1",
            answerSet: answerSet);

        context.Organisations.Add(organisation);
        context.SharedConsents.Add(sharedConsent);
        await context.SaveChangesAsync();

        var repository = FormRepository(context);

        var result = await repository.DeleteAnswerSetAsync(organisationId, answerSetId);
        var foundAnswerSet = await context.FormAnswerSets.FirstAsync(a => a.Guid == answerSetId);
        var sharedConsents = context.SharedConsents
            .Where(s => s.OrganisationId == organisation.Id)
            .ToList();

        result.Should().BeTrue();
        foundAnswerSet.As<FormAnswerSet>().Deleted.Should().BeFalse();
        sharedConsents.Count.Should().Be(2);
        sharedConsents.ElementAt(0).Guid.Should().Be(sharedConsent.Guid);
        sharedConsents.ElementAt(0).AnswerSets.First().Deleted.Should().BeFalse();
        sharedConsents.ElementAt(1).Guid.Should().NotBe(sharedConsent.Guid);
        sharedConsents.ElementAt(1).AnswerSets.First().Deleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetFormAnswerSetsAsync_WhenFormAnswerSetsDoNotExist_ReturnsEmptyList()
    {
        using var repository = FormRepository();

        var nonExistentSectionId = Guid.NewGuid();
        var nonExistentOrganisationId = Guid.NewGuid();

        var foundAnswerSets =
            await repository.GetFormAnswerSetsFromCurrentSharedConsentAsync(nonExistentSectionId,
                nonExistentOrganisationId);

        foundAnswerSets.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFormAnswerSetAsync_WhenFormAnswerSetDoesNotExist_ReturnsNull()
    {
        using var repository = FormRepository();

        var nonExistentAnswerSetId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        var foundAnswerSet =
            await repository.GetFormAnswerSetsFromCurrentSharedConsentAsync(nonExistentAnswerSetId, organisationId);

        foundAnswerSet.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetFormAnswerSetAsync_WhenFormAnswerSetsExists_ReturnsFormAnswerSet()
    {
        await using var context = postgreSql.OrganisationInformationContext();
        var repository = FormRepository(context);

        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var form = GivenForm(formId);
        var section = GivenFormSection(sectionId, form);
        form.Sections.Add(section);
        var question = new FormQuestion
        {
            Guid = questionId,
            Section = section,
            Name = "_Section0" + GetQuestionNumber(),
            Title = "Question 1",
            Caption = "Question Caption",
            Description = "Question 1 desc",
            Type = FormQuestionType.YesOrNo,
            IsRequired = true,
            Options = new FormQuestionOptions(),
            NextQuestion = null,
            NextQuestionAlternative = null,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow,
            SortOrder = 0
        };
        section.Questions.Add(question);

        context.Forms.Add(form);
        await context.SaveChangesAsync();

        var organisation = EntityFactory.GivenOrganisation(organisationId);
        context.Organisations.Add(organisation);
        await context.SaveChangesAsync();

        var sharedConsent = GivenSharedConsent(form: form, organisation: organisation);
        var answerSet = new FormAnswerSet
        {
            Guid = Guid.NewGuid(),
            SharedConsentId = sharedConsent.Id,
            SharedConsent = sharedConsent,
            SectionId = section.Id,
            Section = section,
            Answers = new List<FormAnswer>(),
            Deleted = false,
            FurtherQuestionsExempted = false,
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
            AddressValue = new FormAddress
            {
                StreetAddress = "456 Elm St",
                Locality = "London",
                PostalCode = "G67890",
                CountryName = "UK",
                Country = "GB"
            },
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        answerSet.Answers.Add(formAnswer);

        context.FormAnswerSets.Add(answerSet);
        await context.SaveChangesAsync();

        var foundAnswerSet =
            await repository.GetFormAnswerSetsFromCurrentSharedConsentAsync(sectionId, organisation.Guid);

        foundAnswerSet.Should().NotBeNull();
        foundAnswerSet.Should().HaveCount(1);
        foundAnswerSet[0].Answers.Should().HaveCount(1);
        var foundAnswer = foundAnswerSet[0].Answers.First();
        foundAnswer.Question.Should().Be(question);
        foundAnswer.BoolValue.Should().BeTrue();
    }

    [Fact]
    public async Task GetQuestionsAsync_WhenOptionsAreSimple_ReturnsCorrectOptions()
    {
        await using var context = postgreSql.OrganisationInformationContext();
        using var repository = FormRepository(context);
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var form = GivenForm(formId);
        var section = GivenFormSection(sectionId, form);
        form.Sections.Add(section);

        var questionId = Guid.NewGuid();
        var choiceId = Guid.NewGuid();

        var simpleOptions = new FormQuestionOptions
        {
            Choices = new List<FormQuestionChoice>
            {
                new()
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
            Name = "_Section0" + GetQuestionNumber(),
            Title = "Question with Simple Options",
            Caption = "Question Caption",
            Description = "This is a test question with simple options.",
            Type = FormQuestionType.SingleChoice,
            IsRequired = true,
            NextQuestion = null,
            NextQuestionAlternative = null,
            Options = simpleOptions,
            SortOrder = 0
        };

        section.Questions.Add(question);
        await context.Forms.AddAsync(form);
        await context.SaveChangesAsync();

        var foundQuestions = await repository.GetQuestionsAsync(sectionId);

        foundQuestions.Should().NotBeEmpty();
        foundQuestions.Should().HaveCount(1);

        var foundQuestion = foundQuestions.First();
        foundQuestion.Options.Should().NotBeNull();

        foundQuestion.Options.Choices.Should().NotBeNull().And.HaveCount(1);

        var foundChoice = foundQuestion.Options.Choices?.First();
        foundChoice.Should().BeEquivalentTo(simpleOptions.Choices.First(), config => config.Excluding(ctx => ctx.Id));
    }

    private static FormQuestion GivenYesOrNoQuestion(FormSection section)
    {
        return GivenFormQuestion(section: section, type: FormQuestionType.YesOrNo);
    }

    private IFormRepository FormRepository(OrganisationInformationContext? context = null)
    {
        return new DatabaseFormRepository(context ?? postgreSql.OrganisationInformationContext());
    }
}