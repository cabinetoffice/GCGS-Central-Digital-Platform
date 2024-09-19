using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.Forms.WebApi.Tests.AutoMapper;
using CO.CDP.Forms.WebApi.UseCase;
using FluentAssertions;
using Moq;

namespace CO.CDP.Forms.WebApi.Tests.UseCase;

public class GetFormSectionQuestionsUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IFormRepository> _repository = new();
    private GetFormSectionQuestionsUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task ItReturnsNullIfNoSectionIsFound()
    {
        var found = await UseCase.Execute((It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()));

        found.Should().BeNull();
    }

    [Fact]
    public async Task ItReturnsTheSectionWithQuestions()
    {
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        var form = new CO.CDP.OrganisationInformation.Persistence.Forms.Form
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Sample Form",
            Version = "1.0",
            IsRequired = true,
            Scope = CO.CDP.OrganisationInformation.Persistence.Forms.FormScope.SupplierInformation,
            Sections = new List<CO.CDP.OrganisationInformation.Persistence.Forms.FormSection>()
        };

        var section = new CO.CDP.OrganisationInformation.Persistence.Forms.FormSection
        {
            Id = 1,
            Guid = sectionId,
            Title = "Financial Information",
            FormId = form.Id,
            Form = form,
            Questions = new List<CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestion>(),
            Type = CO.CDP.OrganisationInformation.Persistence.Forms.FormSectionType.Standard,
            AllowsMultipleAnswerSets = true,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow,
            Configuration = new OrganisationInformation.Persistence.Forms.FormSectionConfiguration
            {
                PluralSummaryHeadingFormat = "You have added {0} files",
                SingularSummaryHeading = "You have added 1 file",
                AddAnotherAnswerLabel = "Add another file?",
                RemoveConfirmationCaption = "Economic and Financial Standing",
                RemoveConfirmationHeading = "Are you sure you want to remove this file?"
            }
        };

        var questions = new List<CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestion>
        {
            new CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestion
            {
                Id = 1,
                Guid = Guid.NewGuid(),
                Caption = "Page caption",
                Name = "_Section01",
                Title = "The financial information you will need.",
                Description = "You will need to upload accounts or statements for your 2 most recent financial years. If you do not have 2 years, you can upload your most recent financial year. You will need to enter the financial year end date for the information you upload.",
                Type = CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType.NoInput,
                IsRequired = true,
                NextQuestion = null,
                NextQuestionAlternative = null,
                Options = new CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionOptions(),
                Section = section,
            },
            new CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestion
            {
                Id = 2,
                Guid = Guid.NewGuid(),
                Caption = "Page caption",
                Name = "_Section01",
                Title = "Were your accounts audited?.",
                Description = "",
                Type = CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType.YesOrNo,
                IsRequired = true,
                NextQuestion = null,
                NextQuestionAlternative = null,
                Options = new CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionOptions(),
                Section = section,
            }
        };
        section.Questions = questions;
        form.Sections.Add(section);

        _repository.Setup(r => r.GetSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(section);

        var result = await UseCase.Execute((formId, sectionId, organisationId));

        result?.Questions.Should().HaveCount(2);

        result?.Section.Should().BeEquivalentTo(new FormSection
        {
            Id = sectionId,
            Title = "Financial Information",
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
        });

        result?.Questions.Should().BeEquivalentTo(new List<FormQuestion>
        {
            new FormQuestion
            {
                Id = questions[0].Guid,
                Title = questions[0].Title,
                Caption = questions[0].Caption,
                Description = questions[0].Description,
                Type = FormQuestionType.NoInput,
                IsRequired = questions[0].IsRequired,
                Options = new FormQuestionOptions()
            },
            new FormQuestion
            {
                Id = questions[1].Guid,
                Title = questions[1].Title,
                Caption = questions[0].Caption,
                Description = questions[1].Description,
                Type = FormQuestionType.YesOrNo,
                IsRequired = questions[1].IsRequired,
                Options = new FormQuestionOptions()
            }
        });
    }
}