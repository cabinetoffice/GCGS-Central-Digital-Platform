using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;

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
    public async Task GetFormAnswerSetAsync_WhenFormAnswerSetDoesNotExist_ReturnsNull()
    {
        using var repository = FormRepository();

        var nonExistentAnswerSetId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        var foundAnswerSet = await repository.GetFormAnswerSetsAsync(nonExistentAnswerSetId, organisationId);

        foundAnswerSet.Should().BeNull();
    }


    private static Organisation GivenOrganisation(Guid organisationId)
    {
        var tenant = new Tenant
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Test Tenant"
        };

        return new Organisation
        {
            Id = 1,
            Guid = organisationId,
            Name = "Test Organisation",
            Tenant = tenant
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