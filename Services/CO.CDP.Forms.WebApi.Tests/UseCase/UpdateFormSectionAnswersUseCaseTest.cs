using CO.CDP.AwsServices;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.Forms.WebApi.Tests.AutoMapper;
using CO.CDP.Forms.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using static CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType;
using static CO.CDP.OrganisationInformation.Persistence.Forms.FormSectionType;
using static CO.CDP.OrganisationInformation.Persistence.Forms.SubmissionState;
using static CO.CDP.OrganisationInformation.Persistence.Tests.Factories.SharedConsentFactory;
using Persistence = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.Forms.WebApi.Tests.UseCase;

public class UpdateFormSectionAnswersUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IFormRepository> _repository = new();
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();

    private UpdateFormSectionAnswersUseCase UseCase => new(
        _repository.Object,
        _organisationRepository.Object,
        mapperFixture.Mapper
    );


    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationIsNotFound()
    {
        var organisationId = Guid.NewGuid();

        GivenOrganisationDoesNotExist(organisationId: organisationId);

        var updateFormSectionAnswers = new UpdateFormSectionAnswers { Answers = [], FurtherQuestionsExempted = true };

        var act = async () =>
        {
            await UseCase.Execute((Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), organisationId, updateFormSectionAnswers));
        };

        await act.Should().ThrowAsync<UnknownOrganisationException>();
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownSectionException_WhenSectionIsNotFound()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var updateFormSectionAnswers = new UpdateFormSectionAnswers
        {
            Answers = new List<FormAnswer>
            {
                new() { Id = Guid.NewGuid(), QuestionId = Guid.NewGuid(), BoolValue = true }
            }
        };
        var command = (
            formId,
            sectionId,
            answerSetId: Guid.NewGuid(),
            organisationId,
            updateFormSectionAnswers
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
        var form = GivenForm();
        var updateFormSectionAnswers = new UpdateFormSectionAnswers
        {
            Answers = new List<FormAnswer>
            {
                new() { Id = Guid.NewGuid(), QuestionId = Guid.NewGuid()}
            }
        };
        var command = (
            formId: form.Guid,
            sectionId,
            answerSetId: Guid.NewGuid(),
            organisationId,
            updateFormSectionAnswers
        );

        GivenOrganisationExists(organisationId: organisationId);
        GivenFormSectionExists(sectionId: sectionId, form: form);

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
        var updateFormSectionAnswers = new UpdateFormSectionAnswers
        {
            Answers =
            [
                new() { Id = Guid.NewGuid(), QuestionId = questionId, BoolValue = true}
            ]
        };
        var command = (
            section.Form.Guid,
            sectionId,
            existingAnswerSet.Guid,
            organisationId,
            updateFormSectionAnswers);

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
        var updateFormSectionAnswers = new UpdateFormSectionAnswers
        {
            Answers =
           [
               new() { Id = Guid.NewGuid(), QuestionId = question.Guid, BoolValue = true}
           ]
        };

        var command = (formId: section.Form.Guid, sectionId: section.Guid, answerSetId: Guid.NewGuid(),
            organisationId: organisation.Guid, updateFormSectionAnswers);

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
        var updateFormSectionAnswers = new UpdateFormSectionAnswers
        {
            Answers =
           [
               new() { Id = Guid.NewGuid(), QuestionId = question.Guid, BoolValue = true}
           ]
        };

        var answerSetId = Guid.NewGuid();

        var command = (formId: section.Form.Guid, sectionId: section.Guid, answerSetId,
            organisationId: organisation.Guid, updateFormSectionAnswers);

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

        var sharedConsent = GivenSharedConsentExists(organisation, form, state: Submitted);
        var answerSet = GivenAnswerSet(sharedConsent: sharedConsent, section: section);
        var answerSetId = answerSet.Guid;
        var answerGuid = Guid.NewGuid();
        var updateFormSectionAnswers = new UpdateFormSectionAnswers
        {
            Answers =
          [
              new() { Id = answerGuid, QuestionId = question.Guid, BoolValue = true }
          ]
        };
        var command = (formId: section.Form.Guid, sectionId: section.Guid, answerSetId,
            organisationId: organisation.Guid,
            updateFormSectionAnswers);

        await UseCase.Execute(command);

        _repository.Verify(r => r.SaveSharedConsentAsync(It.Is<Persistence.SharedConsent>(sc =>
            sc.Guid != sharedConsent.Guid &&
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
        var form = GivenForm();
        var standardSection = GivenFormSectionExists(sectionId: Guid.NewGuid(), form: form, type: Standard);
        var declarationSection = GivenFormSectionExists(sectionId: Guid.NewGuid(), form: form, type: Declaration);
        var question = GivenFormQuestion(questionId: Guid.NewGuid(), section: standardSection);
        GivenFormQuestion(questionId: Guid.NewGuid(), section: declarationSection);
        var answerGuid = Guid.NewGuid();
        var sharedConsent = GivenSharedConsentExists(organisation, form, state: Submitted);
        var answerSet = GivenAnswerSet(sharedConsent: sharedConsent, section: standardSection);
        GivenAnswerSet(sharedConsent: sharedConsent, section: declarationSection);
        var answerSetId = answerSet.Guid;
        var updateFormSectionAnswers = new UpdateFormSectionAnswers
        {
            Answers =
          [
              new() { Id = answerGuid, QuestionId = question.Guid, BoolValue = true }
          ]
        };
        var command = (formId: form.Guid, sectionId: standardSection.Guid, answerSetId,
            organisationId: organisation.Guid,
            updateFormSectionAnswers);

        await UseCase.Execute(command);

        _repository.Verify(r => r.SaveSharedConsentAsync(It.Is<Persistence.SharedConsent>(sc =>
            sc.Guid != sharedConsent.Guid &&
            sc.SubmissionState == default &&
            sc.ShareCode == default &&
            sc.AnswerSets.Count == 1 &&
            sc.AnswerSets.First().Guid != answerSetId &&
            sc.AnswerSets.First().Answers.Count == 1 &&
            sc.AnswerSets.First().Answers.First().Question.Guid == question.Guid &&
            sc.AnswerSets.First().Answers.First().Guid != answerGuid &&
            sc.AnswerSets.All(x => x.Section.Type != Declaration))));
    }

    [Fact]
    public async Task Execute_ShouldSuccessfullyCopyWithNonDeclarationAnswersSharedConsent()
    {
        var organisation = GivenOrganisationExists(organisationId: Guid.NewGuid());
        var form = GivenForm();
        var standardSection = GivenFormSectionExists(sectionId: Guid.NewGuid(), form: form);
        var declarationSection = GivenFormSectionExists(sectionId: Guid.NewGuid(), form: form, type: Declaration);
        var question = GivenFormQuestion(questionId: Guid.NewGuid(), section: standardSection);
        GivenFormQuestion(questionId: Guid.NewGuid(), section: declarationSection);
        var answerGuid = Guid.NewGuid();
        var sharedConsent = GivenSharedConsentExists(organisation, standardSection.Form, state: Submitted);
        var answerSet = GivenAnswerSet(sharedConsent: sharedConsent, section: standardSection);
        GivenAnswerSet(sharedConsent: sharedConsent, section: declarationSection);
        var answerSetId = answerSet.Guid;
        var updateFormSectionAnswers = new UpdateFormSectionAnswers
        {
            Answers =
          [
              new() { Id = answerGuid, QuestionId = question.Guid, BoolValue = true }
          ]
        };
        var command = (formId: standardSection.Form.Guid, sectionId: standardSection.Guid, answerSetId,
            organisationId: organisation.Guid,
            updateFormSectionAnswers);


        await UseCase.Execute(command);

        _repository.Verify(r => r.SaveSharedConsentAsync(It.Is<Persistence.SharedConsent>(sc =>
            sc.Guid != sharedConsent.Guid &&
            sc.SubmissionState == default &&
            sc.ShareCode == default &&
            sc.AnswerSets.Count == 1 &&
            sc.AnswerSets.All(x => x.Section.Type != Declaration))));
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
            GivenAnswer(question: GivenFormQuestion(section: section, type: SingleChoice), jsonValue: "{\"json\": true}"),
        };
        var updateFormSectionAnswers = new UpdateFormSectionAnswers
        {
            Answers = new List<FormAnswer>
            {
                new() { Id = answers[0].Guid, QuestionId = answers[0].Question.Guid, BoolValue = true },
                new() { Id = answers[1].Guid, QuestionId = answers[1].Question.Guid, TextValue = "My new answer" },
                new() { Id = answers[2].Guid, QuestionId = answers[2].Question.Guid, DateValue = new DateTime(2025, 1, 12) },
                new() { Id = answers[3].Guid, QuestionId = answers[3].Question.Guid, TextValue = "my-new-photo.jpg" },
                new() { Id = answers[4].Guid, QuestionId = answers[4].Question.Guid, JsonValue = "{\"json\": true}" },
            }
        };
        var answerSet = GivenAnswerSet(sharedConsent: sharedConsent, section: section, answers: answers);
        var command = (
            formId: section.Form.Guid,
            sectionId: section.Guid,
            answerSetId: answerSet.Guid,
            organisationId: organisation.Guid,
            updateFormSectionAnswers);

        await UseCase.Execute(command);

        _repository.Verify(r => r.SaveSharedConsentAsync(It.Is<Persistence.SharedConsent>(sc =>
            sc.Guid != sharedConsent.Guid &&
            sc.SubmissionState == Draft &&
            sc.AnswerSets.Count == 1 &&
            sc.AnswerSets.First().Answers.ElementAt(0).BoolValue == true &&
            sc.AnswerSets.First().Answers.ElementAt(1).TextValue == "My new answer" &&
            sc.AnswerSets.First().Answers.ElementAt(2).DateValue == new DateTime(2025, 1, 12) &&
            sc.AnswerSets.First().Answers.ElementAt(3).TextValue == "my-new-photo.jpg" &&
            sc.AnswerSets.First().Answers.ElementAt(4).JsonValue == "{\"json\": true}"
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
            GivenFormQuestion(section: section, type: FileUpload),
            GivenFormQuestion(section: section, type: SingleChoice)
        };
        var updateFormSectionAnswers = new UpdateFormSectionAnswers
        {
            Answers = new List<FormAnswer>
            {
                new() { Id = Guid.NewGuid(), QuestionId = questions[0].Guid, BoolValue = true },
                new() { Id = Guid.NewGuid(), QuestionId = questions[1].Guid, TextValue = "My new answer" },
                new() { Id = Guid.NewGuid(), QuestionId = questions[2].Guid, DateValue = new DateTime(2025, 1, 12) },
                new() { Id = Guid.NewGuid(), QuestionId = questions[3].Guid, TextValue = "my-new-photo.jpg" },
                new() { Id = Guid.NewGuid(), QuestionId = questions[4].Guid, JsonValue = "{\"json\": true}" },
            }
        };
        var command = (
            formId: section.Form.Guid,
            sectionId: section.Guid,
            answerSetId: Guid.NewGuid(),
            organisationId: organisation.Guid,
            updateFormSectionAnswers);

        await UseCase.Execute(command);

        _repository.Verify(r => r.SaveSharedConsentAsync(It.Is<Persistence.SharedConsent>(sc =>
            sc.Guid != sharedConsent.Guid &&
            sc.SubmissionState == Draft &&
            sc.AnswerSets.Count == 1 &&
            sc.AnswerSets.First().Answers.ElementAt(0).BoolValue == true &&
            sc.AnswerSets.First().Answers.ElementAt(1).TextValue == "My new answer" &&
            sc.AnswerSets.First().Answers.ElementAt(2).DateValue == new DateTime(2025, 1, 12) &&
            sc.AnswerSets.First().Answers.ElementAt(3).TextValue == "my-new-photo.jpg" &&
            sc.AnswerSets.First().Answers.ElementAt(4).JsonValue == "{\"json\": true}"
        )));
    }

    [Fact]
    public async Task Execute_ShouldCopySharedConsentWithExistingAnswersWhileUpdatingChangedAnswers()
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
            GivenAnswer(question: GivenFormQuestion(section: section, type: CheckBox), boolValue: true),
            GivenAnswer(question: GivenFormQuestion(section: section, type: SingleChoice), optionValue: "Option-1"),
            GivenAnswer(question: GivenFormQuestion(section: section, type: Persistence.FormQuestionType.Address),
                addressValue: new Persistence.FormAddress
                {
                    StreetAddress = "10 Yellow Lane",
                    Locality = "London",
                    PostalCode = "SW19 8AR",
                    CountryName = "United Kingodm",
                    Country = "UK"
                }),
            GivenAnswer(question: GivenFormQuestion(section: section, type: SingleChoice), jsonValue: "{\"json\": true}"),
        };

        var updateFormSectionAnswers = new UpdateFormSectionAnswers
        {
            Answers = new List<FormAnswer>
            {
                new() { Id = answers[1].Guid, QuestionId = answers[1].Question.Guid, TextValue = "My new answer" },
            }
        };

        var answerSet = GivenAnswerSet(sharedConsent: sharedConsent, section: section, answers: answers);
        var command = (
            formId: section.Form.Guid,
            sectionId: section.Guid,
            answerSetId: answerSet.Guid,
            organisationId: organisation.Guid,
            updateFormSectionAnswers);

        await UseCase.Execute(command);

        _repository.Verify(r => r.SaveSharedConsentAsync(It.Is<Persistence.SharedConsent>(sc =>
            sc.Guid != sharedConsent.Guid &&
            sc.SubmissionState == Draft &&
            sc.AnswerSets.Count == 1 &&
            sc.AnswerSets.First().Answers.Count == 8 &&
            sc.AnswerSets.First().Answers.ElementAt(0).BoolValue == false &&
            sc.AnswerSets.First().Answers.ElementAt(1).TextValue == "My new answer" &&
            sc.AnswerSets.First().Answers.ElementAt(2).DateValue == new DateTime(2024, 12, 31) &&
            sc.AnswerSets.First().Answers.ElementAt(3).TextValue == "my-photo.jpg" &&
            sc.AnswerSets.First().Answers.ElementAt(4).BoolValue == true &&
            sc.AnswerSets.First().Answers.ElementAt(5).OptionValue == "Option-1" &&
            sc.AnswerSets.First().Answers.ElementAt(6).AddressValue != null &&
            sc.AnswerSets.First().Answers.ElementAt(7).JsonValue == "{\"json\": true}"
        )));
    }

    [Fact]
    public async Task Execute_ShouldReturnExistingAnswerSet_WhenFurtherQuestionsExemptedIsTrue()
    {
        var organisation = GivenOrganisationExists(organisationId: Guid.NewGuid());
        var section = GivenFormSectionExists(sectionId: Guid.NewGuid());
        var sharedConsent = GivenSharedConsentExists(organisation, section.Form);
        var existingAnswerSet = GivenAnswerSet(sharedConsent, section);

        existingAnswerSet.FurtherQuestionsExempted = true;
        _repository.Setup(r => r.GetFormAnswerSetsFromCurrentSharedConsentAsync(section.Guid, sharedConsent.Organisation.Guid))
            .ReturnsAsync(new List<Persistence.FormAnswerSet> { existingAnswerSet });

        var command = (
            formId: section.Form.Guid,
            sectionId: section.Guid,
            answerSetId: existingAnswerSet.Guid,
            organisationId: organisation.Guid,
            new UpdateFormSectionAnswers { Answers = [], FurtherQuestionsExempted = true }
        );

        var result = await UseCase.Execute(command);
        existingAnswerSet.Deleted.Should().BeFalse();
        _repository.Verify(r => r.SaveSharedConsentAsync(sharedConsent), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldDeleteExistingAnswerSet_WhenFurtherQuestionsExemptedIsFalse()
    {
        var organisation = GivenOrganisationExists(organisationId: Guid.NewGuid());
        var section = GivenFormSectionExists(sectionId: Guid.NewGuid());
        var sharedConsent = GivenSharedConsentExists(organisation, section.Form);

        var existingAnswerSet = GivenAnswerSet(sharedConsent, section);
        existingAnswerSet.FurtherQuestionsExempted = true;
        _repository.Setup(r => r.GetFormAnswerSetsFromCurrentSharedConsentAsync(section.Guid, sharedConsent.Organisation.Guid))
            .ReturnsAsync(new List<Persistence.FormAnswerSet> { existingAnswerSet });

        var command = (
            formId: section.Form.Guid,
            sectionId: section.Guid,
            answerSetId: Guid.NewGuid(),
            organisationId: organisation.Guid,
            new UpdateFormSectionAnswers { Answers = [], FurtherQuestionsExempted = false }
        );

        await UseCase.Execute(command);
        existingAnswerSet.Deleted.Should().BeTrue();
        _repository.Verify(r => r.SaveSharedConsentAsync(sharedConsent), Times.Once);
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
        List<Persistence.FormQuestion>? questions = null,
        Persistence.FormSectionType type = Standard
    )
    {
        var section = GivenFormSection(sectionId, form ?? GivenForm(), questions, type);
        _repository.Setup(r => r.GetSectionAsync(section.Form.Guid, section.Guid)).ReturnsAsync(section);
        return section;
    }

    private void GivenFormSectionDoesNotExist(Guid formId, Guid sectionId)
    {
        _repository.Setup(r => r.GetSectionAsync(formId, sectionId)).ReturnsAsync((Persistence.FormSection?)null);
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
}