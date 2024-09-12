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

        var (mappedSharedConsent, mappedAnswerSetId, mappedAnswers) =
            SharedConsentMapper.Map(sharedConsent, answerSetId, answers);

        mappedSharedConsent.Should().Be(sharedConsent);
        mappedAnswerSetId.Should().Be(answerSetId);
        mappedAnswers.Should().BeEquivalentTo(answers);
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

        var (mappedSharedConsent, mappedAnswerSetId, mappedAnswers) =
            SharedConsentMapper.Map(sharedConsent, answerSetId, answers);

        mappedSharedConsent.Should().NotBeEquivalentTo(sharedConsent);
        mappedSharedConsent.Id.Should().Be(default);
        mappedSharedConsent.Guid.Should().NotBe(sharedConsent.Guid);
        mappedSharedConsent.OrganisationId.Should().Be(sharedConsent.OrganisationId);
        mappedSharedConsent.FormId.Should().Be(sharedConsent.FormId);
        mappedSharedConsent.SubmissionState.Should().Be(Draft);
        mappedSharedConsent.SubmittedAt.Should().BeNull();
        mappedSharedConsent.FormVersionId.Should().Be(sharedConsent.FormVersionId);
        mappedSharedConsent.ShareCode.Should().Be(sharedConsent.ShareCode);
        mappedAnswerSetId.Should().Be(answerSetId, "AnswerSet did not exist");
        mappedAnswers.Should().BeEquivalentTo(answers, "Answer did not exist");
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

        var (mappedSharedConsent, mappedAnswerSetId, mappedAnswers) =
            SharedConsentMapper.Map(sharedConsent, answerSet.Guid, answers);

        mappedSharedConsent.Should().NotBeEquivalentTo(sharedConsent);
        mappedSharedConsent.Id.Should().Be(default);
        mappedSharedConsent.Guid.Should().NotBe(sharedConsent.Guid);
        mappedSharedConsent.OrganisationId.Should().Be(sharedConsent.OrganisationId);
        mappedSharedConsent.FormId.Should().Be(sharedConsent.FormId);
        mappedSharedConsent.SubmissionState.Should().Be(Draft);
        mappedSharedConsent.SubmittedAt.Should().BeNull();
        mappedSharedConsent.FormVersionId.Should().Be(sharedConsent.FormVersionId);
        mappedSharedConsent.ShareCode.Should().Be(sharedConsent.ShareCode);
        mappedAnswerSetId.Should().NotBe(answerSet.Guid, "AnswerSet exists and must be cloned");
        mappedAnswers.Should().NotBeEquivalentTo(answers, "Answer exists and must be cloned");
        mappedAnswers.First().Id.Should().NotBe(answers.First().Id);
        mappedAnswers.First().QuestionId.Should().Be(answers.First().QuestionId);
        mappedAnswers.First().BoolValue.Should().Be(answers.First().BoolValue);
    }
}