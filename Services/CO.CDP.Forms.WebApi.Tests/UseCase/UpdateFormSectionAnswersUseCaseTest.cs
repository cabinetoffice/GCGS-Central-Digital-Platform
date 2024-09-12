using CO.CDP.AwsServices;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.Forms.WebApi.Tests.AutoMapper;
using CO.CDP.Forms.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using static CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType;
using static CO.CDP.OrganisationInformation.Persistence.Forms.SubmissionState;
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
        var command = (
            formId,
            sectionId,
            answerSetId: Guid.NewGuid(),
            organisationId,
            answers: new List<FormAnswer>
            {
                new() { Id = Guid.NewGuid(), QuestionId = Guid.NewGuid(), BoolValue = true }
            }
        );

        GivenOrganisationExists(organisationId: organisationId);
        GivenFormSectionDoesNotExist(formId: formId, sectionId: sectionId);

        var act = async () => await UseCase.Execute(command);

        await act.Should().ThrowAsync<UnknownSectionException>();
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownQuestionsException_WhenOneOrMoreQuestionsAreInvalid()
    {
        var organisationId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var command = (
            formId: Guid.NewGuid(),
            sectionId,
            answerSetId: Guid.NewGuid(),
            organisationId,
            invalidAnswers: new List<FormAnswer>
            {
                new() { Id = Guid.NewGuid(), QuestionId = Guid.NewGuid() }
            }
        );

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
        var command = (section.Form.Guid, sectionId, existingAnswerSet.Guid, organisationId,
            answers: new List<FormAnswer>
            {
                new() { Id = Guid.NewGuid(), QuestionId = questionId, BoolValue = true }
            });

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

    [Fact]
    public async Task Execute_ShouldSuccessfullyCopySharedConsent()
    {
        var organisation = GivenOrganisationExists(organisationId: Guid.NewGuid());
        var form = GivenForm();
        var section = GivenFormSectionExists(sectionId: Guid.NewGuid(), form: form);
        var question = GivenFormQuestion(questionId: Guid.NewGuid(), section: section);

        var sharedConsent = GivenSharedConsentExists(organisation, form);
        sharedConsent.SubmissionState = Submitted;
        var answerSet = GivenAnswerSet(sharedConsent: sharedConsent, section: section);
        var answerSetId = answerSet.Guid;
        var answerGuid = Guid.NewGuid();
        var answers = new List<FormAnswer>
        {
            new() { Id = answerGuid, QuestionId = question.Guid, BoolValue = true }
        };
        var command = (formId: section.Form.Guid, sectionId: section.Guid, answerSetId,
            organisationId: organisation.Guid, answers);

        var sharedConsentGuid = sharedConsent.Guid;
        _repository.Setup(useCase => useCase.GetSharedConsentWithAnswersAsync(section.Form.Guid, organisation.Guid))
            .ReturnsAsync(sharedConsent);

        await UseCase.Execute(command);

        _repository.Verify(r => r.SaveSharedConsentAsync(It.Is<Persistence.SharedConsent>(sc =>
            sc.Guid != sharedConsentGuid &&
            sc.SubmissionState == default &&
            sc.ShareCode == default &&
            sc.AnswerSets.Count == 1 &&
            sc.AnswerSets.First().Guid != answerSetId &&
            sc.AnswerSets.First().Answers.Count == 1 &&
            sc.AnswerSets.First().Answers.First().Question.Guid == question.Guid &&
            sc.AnswerSets.First().Answers.First().Guid != answerGuid)));
    }

    [Fact]
    public async Task Execute_ShouldSuccessfullyCopyWithoutDeclarationAnswersSharedConsent()
    {
        var organisation = GivenOrganisationExists(organisationId: Guid.NewGuid());
        var section = GivenFormSectionExists(sectionId: Guid.NewGuid());
        var question = GivenFormQuestion(questionId: Guid.NewGuid(), section: section);
        var answerGuid = Guid.NewGuid();
        var sharedConsent =
            EntityFactory.GetSharedConsentWithDeclarationOnlyAnswerSets(organisation.Id, organisation.Guid,
                section.Form.Guid);
        sharedConsent.SubmissionState = Submitted;
        var answerSet = GivenAnswerSet(sharedConsent: sharedConsent, section: section);
        var answerSetId = answerSet.Guid;
        var answers = new List<FormAnswer>
        {
            new() { Id = answerGuid, QuestionId = question.Guid, BoolValue = true }
        };
        var command = (formId: section.Form.Guid, sectionId: section.Guid, answerSetId,
            organisationId: organisation.Guid, answers);

        var sharedConsentGuid = sharedConsent.Guid;
        _repository.Setup(useCase => useCase.GetSharedConsentWithAnswersAsync(section.Form.Guid, organisation.Guid))
            .ReturnsAsync(sharedConsent);

        await UseCase.Execute(command);

        _repository.Verify(r => r.SaveSharedConsentAsync(It.Is<Persistence.SharedConsent>(sc =>
            sc.Guid != sharedConsentGuid &&
            sc.SubmissionState == default &&
            sc.ShareCode == default &&
            sc.AnswerSets.Count == 1 &&
            sc.AnswerSets.First().Guid != answerSetId &&
            sc.AnswerSets.First().Answers.Count == 1 &&
            sc.AnswerSets.First().Answers.First().Question.Guid == question.Guid &&
            sc.AnswerSets.First().Answers.First().Guid != answerGuid &&
            sc.AnswerSets.All(x => x.Section.Type != Persistence.FormSectionType.Declaration))));
    }

    [Fact]
    public async Task Execute_ShouldSuccessfullyCopyWithNonDeclarationAnswersSharedConsent()
    {
        var organisation = GivenOrganisationExists(organisationId: Guid.NewGuid());
        var section = GivenFormSectionExists(sectionId: Guid.NewGuid());
        var question = GivenFormQuestion(questionId: Guid.NewGuid(), section: section);
        var answerGuid = Guid.NewGuid();
        var answers = new List<FormAnswer>
        {
            new() { Id = answerGuid, QuestionId = question.Guid, BoolValue = true }
        };
        var answerSetId = Guid.NewGuid();
        var command = (formId: section.Form.Guid, sectionId: section.Guid, answerSetId,
            organisationId: organisation.Guid, answers);

        var sharedConsent =
            EntityFactory.GetSharedConsentWithMixedTypeDeclarationAnswerSets(organisation.Id, organisation.Guid,
                section.Form.Guid);
        sharedConsent.SubmissionState = Submitted;

        var sharedConsentGuid = sharedConsent.Guid;
        _repository.Setup(useCase => useCase.GetSharedConsentWithAnswersAsync(section.Form.Guid, organisation.Guid))
            .ReturnsAsync(sharedConsent);

        await UseCase.Execute(command);

        _repository.Verify(r => r.SaveSharedConsentAsync(It.Is<Persistence.SharedConsent>(sc =>
            sc.Guid != sharedConsentGuid &&
            sc.SubmissionState == default &&
            sc.ShareCode == default &&
            sc.AnswerSets.Count == 2 &&
            sc.AnswerSets.All(x => x.Section.Type != Persistence.FormSectionType.Declaration))));
    }

    [Fact]
    public async Task Execute_ShouldCopySharedConsentWithExistingAnswers()
    {
        var organisation = GivenOrganisationExists(organisationId: Guid.NewGuid());
        var section = GivenFormSectionExists(sectionId: Guid.NewGuid());
        var sharedConsent = GivenSharedConsentExists(organisation, section.Form, state: Submitted);
        var answers = new List<Persistence.FormAnswer>
        {
            GivenAnswer(question: GivenFormQuestion(section: section, type: YesOrNo), boolValue: false),
            GivenAnswer(question: GivenFormQuestion(section: section, type: Text), textValue: "My answer"),
            GivenAnswer(question: GivenFormQuestion(section: section, type: Date),
                dateValue: new DateTime(2024, 12, 31)),
            GivenAnswer(question: GivenFormQuestion(section: section, type: FileUpload), textValue: "my-photo.jpg"),
        };
        var answerSet = GivenAnswerSet(sharedConsent: sharedConsent, section: section, answers: answers);
        var command = (
            formId: section.Form.Guid,
            sectionId: section.Guid,
            answerSetId: answerSet.Guid,
            organisationId: organisation.Guid,
            answers: new List<FormAnswer>
            {
                new() { Id = answers[0].Guid, QuestionId = answers[0].Question.Guid, BoolValue = true },
                new() { Id = answers[1].Guid, QuestionId = answers[1].Question.Guid, TextValue = "My new answer" },
                new() { Id = answers[2].Guid, QuestionId = answers[2].Question.Guid, DateValue = new DateTime(2025, 1, 12) },
                new() { Id = answers[3].Guid, QuestionId = answers[3].Question.Guid, TextValue = "my-new-photo.jpg" },
            });

        _repository.Setup(useCase => useCase.GetSharedConsentWithAnswersAsync(section.Form.Guid, organisation.Guid))
            .ReturnsAsync(sharedConsent);

        await UseCase.Execute(command);

        _repository.Verify(r => r.SaveSharedConsentAsync(It.Is<Persistence.SharedConsent>(sc =>
            sc.Guid != sharedConsent.Guid &&
            sc.SubmissionState == Draft &&
            sc.AnswerSets.Count == 1 &&
            sc.AnswerSets.First().Answers.ElementAt(0).BoolValue == true &&
            sc.AnswerSets.First().Answers.ElementAt(1).TextValue == "My new answer" &&
            sc.AnswerSets.First().Answers.ElementAt(2).DateValue == new DateTime(2025, 1, 12) &&
            sc.AnswerSets.First().Answers.ElementAt(3).TextValue == "my-new-photo.jpg"
        )));
    }

    [Fact]
    public async Task Execute_ShouldCopySharedConsentWithNewAnswers()
    {
        var organisation = GivenOrganisationExists(organisationId: Guid.NewGuid());
        var section = GivenFormSectionExists(sectionId: Guid.NewGuid());
        var sharedConsent = GivenSharedConsentExists(organisation, section.Form, state: Submitted);
        var questions = new List<Persistence.FormQuestion>
        {
            GivenFormQuestion(section: section, type: YesOrNo),
            GivenFormQuestion(section: section, type: Text),
            GivenFormQuestion(section: section, type: Date),
            GivenFormQuestion(section: section, type: FileUpload)
        };
        var command = (
            formId: section.Form.Guid,
            sectionId: section.Guid,
            answerSetId: Guid.NewGuid(),
            organisationId: organisation.Guid,
            answers: new List<FormAnswer>
            {
                new() { Id = Guid.NewGuid(), QuestionId = questions[0].Guid, BoolValue = true },
                new() { Id = Guid.NewGuid(), QuestionId = questions[1].Guid, TextValue = "My new answer" },
                new() { Id = Guid.NewGuid(), QuestionId = questions[2].Guid, DateValue = new DateTime(2025, 1, 12) },
                new() { Id = Guid.NewGuid(), QuestionId = questions[3].Guid, TextValue = "my-new-photo.jpg" },
            });

        _repository.Setup(useCase => useCase.GetSharedConsentWithAnswersAsync(section.Form.Guid, organisation.Guid))
            .ReturnsAsync(sharedConsent);

        await UseCase.Execute(command);

        _repository.Verify(r => r.SaveSharedConsentAsync(It.Is<Persistence.SharedConsent>(sc =>
            sc.Guid != sharedConsent.Guid &&
            sc.SubmissionState == Draft &&
            sc.AnswerSets.Count == 1 &&
            sc.AnswerSets.First().Answers.ElementAt(0).BoolValue == true &&
            sc.AnswerSets.First().Answers.ElementAt(1).TextValue == "My new answer" &&
            sc.AnswerSets.First().Answers.ElementAt(2).DateValue == new DateTime(2025, 1, 12) &&
            sc.AnswerSets.First().Answers.ElementAt(3).TextValue == "my-new-photo.jpg"
        )));
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

    private Persistence.FormSection GivenFormSectionExists(
        Guid sectionId,
        Persistence.Form? form = null,
        List<Persistence.FormQuestion>? questions = null
    )
    {
        var section = GivenFormSection(sectionId, form ?? GivenForm(), questions);
        _repository.Setup(r => r.GetSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(section);
        return section;
    }

    private void GivenFormSectionDoesNotExist(Guid formId, Guid sectionId)
    {
        _repository.Setup(r => r.GetSectionAsync(formId, sectionId)).ReturnsAsync((Persistence.FormSection?)null);
    }

    private static Persistence.FormQuestion GivenFormQuestion(
        Persistence.FormSection section,
        Guid? questionId = null,
        Persistence.FormQuestionType? type = null
    )
    {
        var question = new Persistence.FormQuestion
        {
            Guid = questionId ?? Guid.NewGuid(),
            Title = "Were your accounts audited?",
            Caption = "",
            Description = "",
            Type = type ?? YesOrNo,
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

    private static Persistence.FormSection GivenFormSection(
        Guid sectionId,
        Persistence.Form form,
        List<Persistence.FormQuestion>? questions = null
    )
    {
        var formSection = new Persistence.FormSection
        {
            Id = 1,
            Guid = sectionId,
            Title = "Financial Information",
            FormId = form.Id,
            Form = form,
            Questions = questions ?? [],
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
        questions?.ForEach(q => q.Section = formSection);
        form.Sections.Add(formSection);
        return formSection;
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
        Persistence.Form form,
        Persistence.SubmissionState? state = null)
    {
        var sharedConsent = GivenSharedConsent(organisation, form, state);

        _repository.Setup(r => r.GetSharedConsentWithAnswersAsync(form.Guid, organisation.Guid))
            .ReturnsAsync(sharedConsent);

        return sharedConsent;
    }

    private Persistence.SharedConsent GivenSharedConsent(
        Organisation organisation,
        Persistence.Form form,
        Persistence.SubmissionState? state = null)
    {
        return new Persistence.SharedConsent()
        {
            Guid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            FormId = form.Id,
            Form = form,
            AnswerSets = [],
            SubmissionState = state ?? Draft,
            SubmittedAt = null,
            FormVersionId = "202405",
            ShareCode = null
        };
    }

    private static Persistence.FormAnswer GivenAnswer(
        Persistence.FormQuestion question,
        bool? boolValue = null,
        double? numericValue = null,
        DateTime? dateValue = null,
        DateTime? startValue = null,
        DateTime? endValue = null,
        string? textValue = null,
        string? optionValue = null,
        Persistence.FormAddress? addressValue = null
    )
    {
        return new Persistence.FormAnswer
        {
            Id = 0,
            Guid = Guid.NewGuid(),
            QuestionId = question.Id,
            Question = question,
            FormAnswerSetId = default,
            FormAnswerSet = null,
            BoolValue = boolValue,
            NumericValue = numericValue,
            DateValue = dateValue,
            StartValue = startValue,
            EndValue = endValue,
            TextValue = textValue,
            OptionValue = optionValue,
            AddressValue = addressValue,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow,
        };
    }
}