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
            Questions = [],
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
            Sections = [],
            Type = FormType.Standard
        };
    }
    public async Task GetFormAnswerSetAsync_WhenFormAnswerSetDoesNotExist_ReturnsNull()
    {
        using var repository = FormRepository();

        var nonExistentAnswerSetId = Guid.NewGuid();

        var foundAnswerSet = await repository.GetFormAnswerSetAsync(nonExistentAnswerSetId);

        foundAnswerSet.Should().BeNull();
    }


    [Fact]
    public async Task GetFormSectionAsync_WhenFormSectionDoesNotExist_ReturnsNull()
    {
        using var repository = FormRepository();

        var nonExistentSectionId = Guid.NewGuid();

        var foundSection = await repository.GetFormSectionAsync(nonExistentSectionId);

        foundSection.Should().BeNull();
    }


    private IFormRepository FormRepository()
    {
        return new DatabaseFormRepository(postgreSql.OrganisationInformationContext());
    }
}
