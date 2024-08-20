using CO.CDP.Forms.WebApi.Tests.AutoMapper;
using CO.CDP.Forms.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;
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

        var form = new Form
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Sample Form",
            Version = "1.0",
            IsRequired = true,
            Type = FormType.Standard,
            Scope = FormScope.SupplierInformation,
            Sections = new List<FormSection>()
        };

        var section = new FormSection
        {
            Id = 1,
            Guid = sectionId,
            Title = "Financial Information",
            Form = form,
            Questions = new List<FormQuestion>(),
            AllowsMultipleAnswerSets = true,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            Configuration = new FormSectionConfiguration
            {
                PluralSummaryHeadingFormat = "You have added {0} files",
                SingularSummaryHeading = "You have added 1 file",
                AddAnotherAnswerLabel = "Add another file?",
                RemoveConfirmationCaption = "Economic and Financial Standing",
                RemoveConfirmationHeading = "Are you sure you want to remove this file?"
            }
        };

        var questions = new List<FormQuestion>
        {
            new FormQuestion
            {
                Id = 1,
                Guid = Guid.NewGuid(),
                Caption = "Page caption",
                Title = "The financial information you will need.",
                Description = "You will need to upload accounts or statements for your 2 most recent financial years. If you do not have 2 years, you can upload your most recent financial year. You will need to enter the financial year end date for the information you upload.",
                Type = FormQuestionType.NoInput,
                IsRequired = true,
                NextQuestion = null,
                NextQuestionAlternative = null,
                Options = new FormQuestionOptions(),
                Section = section,
            },
            new FormQuestion
            {
                Id = 2,
                Guid = Guid.NewGuid(),
                Caption = "Page caption",
                Title = "Were your accounts audited?.",
                Description = "",
                Type = FormQuestionType.YesOrNo,
                IsRequired = true,
                NextQuestion = null,
                NextQuestionAlternative = null,
                Options = new FormQuestionOptions(),
                Section = section,
            }
        };
        section.Questions = questions;
        form.Sections.Add(section);

        _repository.Setup(r => r.GetSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(section);

        var result = await UseCase.Execute((formId, sectionId, organisationId));

        result?.Questions.Should().HaveCount(2);

        result?.Section.Should().BeEquivalentTo(new Model.FormSection
        {
            Id = sectionId,
            Title = "Financial Information",
            AllowsMultipleAnswerSets = true,
            Configuration = new Model.FormSectionConfiguration
            {
                PluralSummaryHeadingFormat = "You have added {0} files",
                SingularSummaryHeading = "You have added 1 file",
                AddAnotherAnswerLabel = "Add another file?",
                RemoveConfirmationCaption = "Economic and Financial Standing",
                RemoveConfirmationHeading = "Are you sure you want to remove this file?"
            }
        });

        result?.Questions.Should().BeEquivalentTo(new List<Model.FormQuestion>
        {
            new Model.FormQuestion
            {
                Id = questions[0].Guid,
                Title = questions[0].Title,
                Caption = questions[0].Caption,
                Description = questions[0].Description,
                Type = Model.FormQuestionType.NoInput,
                IsRequired = questions[0].IsRequired,
                Options = new Model.FormQuestionOptions()
            },
            new Model.FormQuestion
            {
                Id = questions[1].Guid,
                Title = questions[1].Title,
                Caption = questions[0].Caption,
                Description = questions[1].Description,
                Type = Model.FormQuestionType.YesOrNo,
                IsRequired = questions[1].IsRequired,
                Options = new Model.FormQuestionOptions()
            }
        });
    }
}