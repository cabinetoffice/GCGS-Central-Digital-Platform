using CO.CDP.AwsServices;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.Forms.WebApi.Tests.AutoMapper;
using CO.CDP.Forms.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.Forms.WebApi.Tests.UseCase;

public class UpdateFormSectionAnswersUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IFormRepository> _repository = new();
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IFileHostManager> _fileHostManager = new();

    private UpdateFormSectionAnswersUseCase UseCase => new(
        _repository.Object,
        _organisationRepository.Object,
        mapperFixture.Mapper,
        _fileHostManager.Object
    );


    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationIsNotFound()
    {
        var organisationId = Guid.NewGuid();

        GivenOrganisationDoesNotExist(organisationId: organisationId);

        var act = async () =>
        {
            await UseCase.Execute((Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), organisationId, []));
        };

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
            new() { Id = Guid.NewGuid(), QuestionId = Guid.NewGuid(), BoolValue = true }
        };
        var command = (formId, sectionId, answerSetId, organisationId, answers);

        GivenOrganisationExists(organisationId: organisationId);
        GivenFormSectionDoesNotExist(formId, sectionId);

        var act = async () => await UseCase.Execute(command);

        await act.Should().ThrowAsync<UnknownSectionException>();
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownQuestionsException_WhenOneOrMoreQuestionsAreInvalid()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();
        var invalidAnswers = new List<FormAnswer>
        {
            new() { Id = Guid.NewGuid(), QuestionId = Guid.NewGuid() }
        };
        var command = (formId, sectionId, answerSetId, organisationId, invalidAnswers);

        GivenOrganisationExists(organisationId: organisationId);
        GivenFormSectionExists(sectionId: sectionId);

        var act = async Task () => await UseCase.Execute(command);

        await act.Should().ThrowAsync<UnknownQuestionsException>();
    }

    [Fact]
    public async Task Execute_ShouldUpdateExistingAnswerSet()
    {
        var organisationId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var section = GivenFormSectionExists(sectionId: sectionId);
        var question = GivenFormQuestion(questionId: questionId, section: section);
        var organisation = GivenOrganisationExists(organisationId: organisationId);
        var sharedConsent = GivenSharedConsentExists(organisation: organisation, form: section.Form);
        var existingAnswerSet = GivenAnswerSet(sharedConsent, section);
        var answers = new List<FormAnswer>
        {
            new() { Id = Guid.NewGuid(), QuestionId = questionId, BoolValue = true }
        };
        var command = (section.Form.Guid, sectionId, existingAnswerSet.Guid, organisationId, answers);

        await UseCase.Execute(command);

        existingAnswerSet.Answers.Should().HaveCount(1);
        existingAnswerSet.Answers.Should().ContainSingle(a => a.BoolValue == true);
        existingAnswerSet.Section.Should().BeSameAs(section);

        var answer = existingAnswerSet.Answers.First();
        answer.Question.Should().Be(question);
        answer.BoolValue.Should().BeTrue();

        _repository.Verify(r => r.SaveSharedConsentAsync(sharedConsent), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldCreateNewAnswerSet()
    {
        var organisation = GivenOrganisationExists(organisationId: Guid.NewGuid());
        var section = GivenFormSectionExists(sectionId: Guid.NewGuid());
        var question = GivenFormQuestion(questionId: Guid.NewGuid(), section: section);
        var sharedConsent = GivenSharedConsentExists(organisation: organisation, form: section.Form);
        var answers = new List<FormAnswer>
        {
            new() { Id = Guid.NewGuid(), QuestionId = question.Guid, BoolValue = true }
        };
        var command = (formId: section.Form.Guid, sectionId: section.Guid, answerSetId: Guid.NewGuid(),
            organisationId: organisation.Guid, answers);

        await UseCase.Execute(command);

        _repository.Verify(r => r.SaveSharedConsentAsync(It.Is<Persistence.SharedConsent>(sc =>
            sc.Guid == sharedConsent.Guid &&
            sc.AnswerSets.Count == 1 &&
            sc.AnswerSets.First().Answers.Count == 1 &&
            sc.AnswerSets.First().Answers.First().Question.Guid == question.Guid &&
            sc.AnswerSets.First().Answers.First().BoolValue == true)), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldCreateNewSharedConsent_WhenNoDraftSharedConsentExists()
    {
        var organisation = GivenOrganisationExists(organisationId: Guid.NewGuid());
        var section = GivenFormSectionExists(sectionId: Guid.NewGuid());
        var question = GivenFormQuestion(questionId: Guid.NewGuid(), section: section);
        var answers = new List<FormAnswer>
        {
            new() { Id = Guid.NewGuid(), QuestionId = question.Guid, BoolValue = true }
        };
        var answerSetId = Guid.NewGuid();
        var command = (formId: section.Form.Guid, sectionId: section.Guid, answerSetId,
            organisationId: organisation.Guid, answers);

        await UseCase.Execute(command);

        _repository.Verify(r => r.SaveSharedConsentAsync(It.Is<Persistence.SharedConsent>(sc =>
            sc.AnswerSets.Count == 1 &&
            sc.AnswerSets.First().Guid == answerSetId &&
            sc.AnswerSets.First().Answers.Count == 1 &&
            sc.AnswerSets.First().Answers.First().Question.Guid == question.Guid &&
            sc.AnswerSets.First().Answers.First().BoolValue == true)), Times.Once);
    }

    private Organisation GivenOrganisationExists(Guid organisationId)
    {
        var organisation = GivenOrganisation(organisationId: organisationId);
        _organisationRepository.Setup(r => r.Find(organisationId)).ReturnsAsync(organisation);
        return organisation;
    }

    private void GivenOrganisationDoesNotExist(Guid organisationId)
    {
        _organisationRepository.Setup(r => r.Find(organisationId)).ReturnsAsync((Organisation?)null);
    }

    private Persistence.FormSection GivenFormSectionExists(Guid sectionId)
    {
        var form = GivenForm();
        var section = GivenFormSection(sectionId, form);
        _repository.Setup(r => r.GetSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(section);
        return section;
    }

    private void GivenFormSectionDoesNotExist(Guid formId, Guid sectionId)
    {
        _repository.Setup(r => r.GetSectionAsync(formId, sectionId)).ReturnsAsync((Persistence.FormSection?)null);
    }

    private static Persistence.FormQuestion GivenFormQuestion(Guid questionId, Persistence.FormSection section)
    {
        var question = new Persistence.FormQuestion
        {
            Guid = questionId,
            Title = "Were your accounts audited?",
            Caption = "",
            Description = "",
            Type = Persistence.FormQuestionType.YesOrNo,
            IsRequired = true,
            NextQuestion = null,
            NextQuestionAlternative = null,
            Options = new Persistence.FormQuestionOptions(),
            Section = section,
        };
        section.Questions.Add(question);
        return question;
    }

    private static Organisation GivenOrganisation(Guid organisationId)
    {
        return new Organisation
        {
            Guid = organisationId,
            Name = $"Test Organisation {organisationId}",
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = $"Test Tenant {organisationId}"
            }
        };
    }

    private static Persistence.FormSection GivenFormSection(Guid sectionId, Persistence.Form form)
    {
        return new Persistence.FormSection
        {
            Id = 1,
            Guid = sectionId,
            Title = "Financial Information",
            FormId = form.Id,
            Form = form,
            Questions = [],
            Type = Persistence.FormSectionType.Standard,
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
    }

    private static Persistence.Form GivenForm()
    {
        return new Persistence.Form
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Sample Form",
            Version = "1.0",
            IsRequired = true,
            Scope = Persistence.FormScope.SupplierInformation,
            Sections = new List<Persistence.FormSection>()
        };
    }

    private static Persistence.FormAnswerSet GivenAnswerSet(
        Persistence.SharedConsent sharedConsent,
        Persistence.FormSection section,
        List<Persistence.FormAnswer>? answers = null
    )
    {
        var existingAnswerSet = new Persistence.FormAnswerSet
        {
            Guid = Guid.NewGuid(),
            SharedConsentId = sharedConsent.Id,
            SharedConsent = sharedConsent,
            SectionId = section.Id,
            Section = section,
            Answers = answers ?? []
        };
        sharedConsent.AnswerSets.Add(existingAnswerSet);
        return existingAnswerSet;
    }

    private Persistence.SharedConsent GivenSharedConsentExists(
        Organisation organisation,
        Persistence.Form form
    )
    {
        var sharedConsent = GivenSharedConsent(organisation, form);

        _repository.Setup(r => r.GetSharedConsentWithAnswersAsync(form.Guid, organisation.Guid))
            .ReturnsAsync(sharedConsent);

        return sharedConsent;
    }

    private Persistence.SharedConsent GivenSharedConsent(
        Organisation organisation,
        Persistence.Form form
    )
    {
        return new Persistence.SharedConsent()
        {
            Guid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            FormId = form.Id,
            Form = form,
            AnswerSets = [],
            SubmissionState = Persistence.SubmissionState.Draft,
            SubmittedAt = null,
            FormVersionId = "202405",
            ShareCode = null
        };
    }
}