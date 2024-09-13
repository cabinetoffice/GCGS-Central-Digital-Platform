using CO.CDP.Forms.WebApi.Model;
using CO.CDP.Forms.WebApi.UseCase.SharedConsent;
using FluentAssertions;
using static CO.CDP.Forms.WebApi.Tests.UseCase.SharedConsentFactory;
using static CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType;
using static CO.CDP.OrganisationInformation.Persistence.Forms.SubmissionState;
using FormQuestion = CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestion;

namespace CO.CDP.Forms.WebApi.Tests.UseCase.SharedConsent;

public class SharedConsentMapperTest
{
    [Fact]
    public void ItReturnsTheOriginalSharedConsentIfItIsInDraftState()
    {
        var sharedConsent = GivenSharedConsent(state: Draft);
        var answerSetId = Guid.NewGuid();
        var answers = new List<FormAnswer>()
        {
            new() { Id = Guid.NewGuid(), QuestionId = Guid.NewGuid(), BoolValue = true }
        };

        var mappedSharedConsent = SharedConsentMapper.Map(sharedConsent);

        mappedSharedConsent.Should().Be(sharedConsent);
        mappedSharedConsent.CreatedFrom.Should().BeNull();
    }

    [Fact]
    public void ItCreatesNewSharedConsentIfItIsInSubmittedStateAndThereAreNoAnswersYet()
    {
        var sharedConsent = GivenSharedConsent(state: Submitted);
        var answerSetId = Guid.NewGuid();
        var questions = new List<FormQuestion>
        {
            GivenFormQuestion(type: YesOrNo),
            GivenFormQuestion(type: NoInput)
        };
        var answers = new List<FormAnswer>()
        {
            new()
            {
                Id = Guid.NewGuid(),
                QuestionId = questions.First().Guid,
                BoolValue = true
            }
        };

        var mappedSharedConsent = SharedConsentMapper.Map(sharedConsent);

        mappedSharedConsent.Should().NotBeEquivalentTo(sharedConsent);
        mappedSharedConsent.Id.Should().Be(default);
        mappedSharedConsent.Guid.Should().NotBe(sharedConsent.Guid);
        mappedSharedConsent.CreatedFrom.Should().Be(sharedConsent.Guid);
        mappedSharedConsent.OrganisationId.Should().Be(sharedConsent.OrganisationId);
        mappedSharedConsent.FormId.Should().Be(sharedConsent.FormId);
        mappedSharedConsent.SubmissionState.Should().Be(Draft);
        mappedSharedConsent.SubmittedAt.Should().BeNull();
        mappedSharedConsent.FormVersionId.Should().Be(sharedConsent.FormVersionId);
        mappedSharedConsent.ShareCode.Should().Be(sharedConsent.ShareCode);
    }

    [Fact]
    public void ItCreatesNewSharedConsentIfItIsInSubmittedStateAndThereAreExistingAnswers()
    {
        var sharedConsent = GivenSharedConsent(state: Submitted);
        var answerSet = GivenAnswerSet(sharedConsent, answers:
        [
            GivenAnswer(boolValue: true),
            GivenAnswer(textValue: "Answer 2")
        ]);
        var answers = new List<FormAnswer>()
        {
            new()
            {
                Id = answerSet.Answers.First().Guid,
                QuestionId = answerSet.Answers.First().Question.Guid,
                BoolValue = true
            }
        };

        var mappedSharedConsent = SharedConsentMapper.Map(sharedConsent);

        mappedSharedConsent.Should().NotBeEquivalentTo(sharedConsent);
        mappedSharedConsent.Id.Should().Be(default);
        mappedSharedConsent.Guid.Should().NotBe(sharedConsent.Guid);
        mappedSharedConsent.CreatedFrom.Should().Be(sharedConsent.Guid);
        mappedSharedConsent.OrganisationId.Should().Be(sharedConsent.OrganisationId);
        mappedSharedConsent.FormId.Should().Be(sharedConsent.FormId);
        mappedSharedConsent.SubmissionState.Should().Be(Draft);
        mappedSharedConsent.SubmittedAt.Should().BeNull();
        mappedSharedConsent.FormVersionId.Should().Be(sharedConsent.FormVersionId);
        mappedSharedConsent.ShareCode.Should().Be(sharedConsent.ShareCode);
        mappedSharedConsent.AnswerSets.First().CreatedFrom.Should().Be(answerSet.Guid);
        mappedSharedConsent.AnswerSets.First().Answers.First().CreatedFrom.Should().Be(answerSet.Answers.First().Guid);
        mappedSharedConsent.AnswerSets.First().Should().NotBe(answerSet.Guid, "AnswerSet exists and must be cloned");
        mappedSharedConsent.AnswerSets.First().Answers.First().Guid.Should().NotBe(answers.First().Id);
        mappedSharedConsent.AnswerSets.First().Answers.First().Question.Guid.Should().Be(answers.First().QuestionId);
        mappedSharedConsent.AnswerSets.First().Answers.First().BoolValue.Should().Be(answers.First().BoolValue);
    }
}