using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CO.CDP.OrganisationInformation.Persistence.Forms.Form;
using static CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestion;
using static CO.CDP.OrganisationInformation.Persistence.Forms.FormAnswer;
using static CO.CDP.OrganisationInformation.Persistence.Organisation;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseFormRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
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
        foundSection.Should().BeEquivalentTo(section, config => config
            .Excluding(ctx => ctx.Id)
            .Excluding(ctx => ctx.CreatedOn)
            .Excluding(ctx => ctx.UpdatedOn)
        );
    }

    [Fact]
    public async Task GetFormSectionAsync_WhenFormSectionDoesNotExist_ReturnsNull()
    {
        using var repository = FormRepository();

        var nonExistentSectionId = Guid.NewGuid();

        var foundSection = await repository.GetFormSectionAsync(nonExistentSectionId);

        foundSection.Should().BeNull();
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
    public async Task GetFormSectionAsync_WhenFormSectionExists_ReturnsFormSection()
    {
        using var repository = FormRepository();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var form = GivenForm(formId);
        var section = GivenSection(sectionId, form);
        form.Sections.Add(section);

        await repository.SaveFormAsync(form);

        var foundSection = await repository.GetFormSectionAsync(sectionId);

        foundSection.Should().NotBeNull();
        foundSection.Should().BeEquivalentTo(section, config => config
            .Excluding(ctx => ctx.Id)
            .Excluding(ctx => ctx.CreatedOn)
            .Excluding(ctx => ctx.UpdatedOn)
        );
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

        var answerSet = new FormAnswerSet
        {
            Guid = answerSetId,
            OrganisationId = organisation.Id,
            Organisation = organisation,
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

        var answerSet = new FormAnswerSet
        {
            Guid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            Section = section,
            Answers = new List<FormAnswer>(),
            Deleted = false,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        var answer = new FormAnswer
        {
            Question = question,
            FormAnswerSet = answerSet,
            BoolValue = true,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        answerSet.Answers.Add(answer);

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

        var answerSet = new FormAnswerSet
        {
            Guid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            Section = section,
            Answers = new List<FormAnswer>(),
            Deleted = false,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        var answer = new FormAnswer
        {
            Question = question,
            FormAnswerSet = answerSet,
            BoolValue = true,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        answerSet.Answers.Add(answer);

        await repository.SaveAnswerSet(answerSet);

        var newAnswer = new FormAnswer
        {
            Question = question,
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
    private static Form GivenForm(Guid formId)
    {
        return new Form
        {
            Guid = formId,
            Name = "Test Form",
            Version = "1.0",
            IsRequired = true,
            Scope = FormScope.SupplierInformation,
            Sections = new List<FormSection>(),
            Type = FormType.Standard
        };
    }

    private static FormSection GivenSection(Guid sectionId, Form form)
    {
        return new FormSection
        {
            Guid = sectionId,
            Form = form,
            Questions = new List<FormQuestion>(),
            Title = "Test Section",
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
    private IFormRepository FormRepository()
    {
        return new DatabaseFormRepository(postgreSql.OrganisationInformationContext());
    }

    private IOrganisationRepository OrganisationRepository()
    {
        return new DatabaseOrganisationRepository(postgreSql.OrganisationInformationContext());
    }
    private async Task<Organisation?> FindOrganisationAsync(IOrganisationRepository organisationRepository, Guid organisationGuid)
    {
        return await organisationRepository.Find(organisationGuid);
    }
}