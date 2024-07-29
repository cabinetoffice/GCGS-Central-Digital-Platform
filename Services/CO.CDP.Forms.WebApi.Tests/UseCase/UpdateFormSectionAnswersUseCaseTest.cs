using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.Forms.WebApi.Tests.AutoMapper;
using CO.CDP.Forms.WebApi.UseCase;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.Forms.WebApi.Tests.UseCase;

public class UpdateFormSectionAnswersUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IFormRepository> _repository = new();
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private UpdateFormSectionAnswersUseCase UseCase => new(_repository.Object, _organisationRepository.Object, mapperFixture.Mapper);


    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationIsNotFound()
    {
        _organisationRepository.Setup(r => r.Find(It.IsAny<Guid>())).ReturnsAsync((Organisation?)null);

        Func<Task> act = async () => await UseCase.Execute((Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new List<FormAnswer>()));

        await act.Should().ThrowAsync<UnknownOrganisationException>();
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownSectionException_WhenSectionIsNotFound()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();
        var answers = new List<FormAnswer>
            {
                new FormAnswer { Id = Guid.NewGuid(), QuestionId = Guid.NewGuid(), BoolValue = true }
            };

        _organisationRepository.Setup(r => r.Find(organisationId)).ReturnsAsync(new Organisation
        {
            Guid = organisationId,
            Name = "Test Organisation",
            Tenant = new Tenant
            {
                Id = 1,
                Guid = Guid.NewGuid(),
                Name = "Test Tenant"
            }
        });
        _repository.Setup(r => r.GetSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((Persistence.FormSection?)null);

        Func<Task> act = async () => await UseCase.Execute((formId, sectionId, answerSetId, organisationId, answers));

        await act.Should().ThrowAsync<UnknownSectionException>();
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownQuestionsException_WhenOneOrMoreQuestionsAreInvalid()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();


        var form = new Persistence.Form
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Sample Form",
            Version = "1.0",
            IsRequired = true,
            Type = Persistence.FormType.Standard,
            Scope = Persistence.FormScope.SupplierInformation,
            Sections = new List<Persistence.FormSection>()
        };

        var section = new Persistence.FormSection
        {
            Id = 1,
            Guid = sectionId,
            Title = "Financial Information",
            Form = form,
            Questions = new List<Persistence.FormQuestion>(),
            AllowsMultipleAnswerSets = true,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow,
            Configuration = new Persistence.FormSectionConfiguration
            {
                PluralSummaryHeadingFormat = "You have added {0} files",
                SingularSummaryHeading = "You have added 1 file",
                AddAnotherAnswerLabel = "Add another file?",
                RemoveConfirmationCaption = "Economic and Financial Standing",
                RemoveConfirmationHeading = "Are you sure you want to remove this file?"
            }
        };

        _organisationRepository.Setup(r => r.Find(organisationId)).ReturnsAsync(new Organisation
        {
            Guid = organisationId,
            Name = "Test Organisation",
            Tenant = new Tenant
            {
                Id = 1,
                Guid = Guid.NewGuid(),
                Name = "Test Tenant"
            }
        });
        _repository.Setup(r => r.GetSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(section);

        var invalidAnswers = new List<FormAnswer>
            {
                new FormAnswer { Id = Guid.NewGuid(), QuestionId = Guid.NewGuid() }
            };

        Func<Task> act = async () => await UseCase.Execute((formId, sectionId, answerSetId, organisationId, invalidAnswers));

        await act.Should().ThrowAsync<UnknownQuestionsException>();
    }

    [Fact]
    public async Task Execute_ShouldUpdateExistingAnswerSet()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var answers = new List<FormAnswer>
    {
        new FormAnswer { Id = Guid.NewGuid(), QuestionId = questionId, BoolValue = true }
    };

        var form = new Persistence.Form
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Sample Form",
            Version = "1.0",
            IsRequired = true,
            Type = Persistence.FormType.Standard,
            Scope = Persistence.FormScope.SupplierInformation,
            Sections = new List<Persistence.FormSection>()
        };

        var section = new Persistence.FormSection
        {
            Id = 1,
            Guid = sectionId,
            Title = "Financial Information",
            Form = form,
            Questions = new List<Persistence.FormQuestion>(),
            AllowsMultipleAnswerSets = true,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow,
            Configuration = new Persistence.FormSectionConfiguration
            {
                PluralSummaryHeadingFormat = "You have added {0} files",
                SingularSummaryHeading = "You have added 1 file",
                AddAnotherAnswerLabel = "Add another file?",
                RemoveConfirmationCaption = "Economic and Financial Standing",
                RemoveConfirmationHeading = "Are you sure you want to remove this file?"
            }
        };

        var question = new Persistence.FormQuestion
        {
            Guid = questionId,
            Title = "Were your accounts audited?",
            Description = "",
            Type = Persistence.FormQuestionType.YesOrNo,
            IsRequired = true,
            NextQuestion = null,
            NextQuestionAlternative = null,
            Options = new Persistence.FormQuestionOptions(),
            Section = section,
        };

        section.Questions.Add(question);

        var existingAnswerSet = new Persistence.FormAnswerSet
        {
            Guid = answerSetId,
            OrganisationId = 1,
            Organisation = new Organisation
            {
                Guid = organisationId,
                Name = "Test Organisation",
                Tenant = new Tenant
                {
                    Id = 1,
                    Guid = Guid.NewGuid(),
                    Name = "Test Tenant"
                }
            },
            Section = section,
            Answers = new List<Persistence.FormAnswer>()
        };

        _organisationRepository.Setup(r => r.Find(organisationId)).ReturnsAsync(new Organisation
        {
            Guid = organisationId,
            Name = "Test Organisation",
            Tenant = new Tenant
            {
                Id = 1,
                Guid = Guid.NewGuid(),
                Name = "Test Tenant"
            }
        });
        _repository.Setup(r => r.GetSectionAsync(formId, sectionId)).ReturnsAsync(section);
        _repository.Setup(r => r.GetFormAnswerSetAsync(sectionId, organisationId, answerSetId)).ReturnsAsync(existingAnswerSet);

        await UseCase.Execute((formId, sectionId, answerSetId, organisationId, answers));

        existingAnswerSet.Answers.Should().HaveCount(1);
        var answer = existingAnswerSet.Answers.First();
        answer.Question.Should().Be(question);
        answer.BoolValue.Should().BeTrue();

        _repository.Verify(r => r.SaveAnswerSet(existingAnswerSet), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldCreateNewAnswerSet()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var answers = new List<FormAnswer>
    {
        new FormAnswer { Id = Guid.NewGuid(), QuestionId = questionId, BoolValue = true }
    };

        var form = new Persistence.Form
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Sample Form",
            Version = "1.0",
            IsRequired = true,
            Type = Persistence.FormType.Standard,
            Scope = Persistence.FormScope.SupplierInformation,
            Sections = new List<Persistence.FormSection>()
        };

        var section = new Persistence.FormSection
        {
            Id = 1,
            Guid = sectionId,
            Title = "Financial Information",
            Form = form,
            Questions = new List<Persistence.FormQuestion>(),
            AllowsMultipleAnswerSets = true,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow,
            Configuration = new Persistence.FormSectionConfiguration
            {
                PluralSummaryHeadingFormat = "You have added {0} files",
                SingularSummaryHeading = "You have added 1 file",
                AddAnotherAnswerLabel = "Add another file?",
                RemoveConfirmationCaption = "Economic and Financial Standing",
                RemoveConfirmationHeading = "Are you sure you want to remove this file?"
            }
        };

        var question = new Persistence.FormQuestion
        {
            Guid = questionId,
            Title = "Were your accounts audited?",
            Description = "",
            Type = Persistence.FormQuestionType.YesOrNo,
            IsRequired = true,
            NextQuestion = null,
            NextQuestionAlternative = null,
            Options = new Persistence.FormQuestionOptions(),
            Section = section,
        };

        section.Questions.Add(question);

        _organisationRepository.Setup(r => r.Find(organisationId)).ReturnsAsync(new Organisation
        {
            Guid = organisationId,
            Name = "Test Organisation",
            Tenant = new Tenant
            {
                Id = 1,
                Guid = Guid.NewGuid(),
                Name = "Test Tenant"
            }
        });
        _repository.Setup(r => r.GetSectionAsync(formId, sectionId)).ReturnsAsync(section);
        _repository.Setup(r => r.GetFormAnswerSetAsync(sectionId, organisationId, answerSetId)).ReturnsAsync((Persistence.FormAnswerSet?)null);

        await UseCase.Execute((formId, sectionId, answerSetId, organisationId, answers));

        _repository.Verify(r => r.SaveAnswerSet(It.Is<Persistence.FormAnswerSet>(aset =>
            aset.Guid == answerSetId &&
            aset.Answers.Count == 1 &&
            aset.Answers.First().Question.Guid == questionId &&
            aset.Answers.First().BoolValue == true)), Times.Once);
    }
}