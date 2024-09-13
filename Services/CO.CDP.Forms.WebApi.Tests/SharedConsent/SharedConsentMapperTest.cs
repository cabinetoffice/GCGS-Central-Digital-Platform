using CO.CDP.Forms.WebApi.SharedConsent;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using FluentAssertions;
using static CO.CDP.Forms.WebApi.Tests.UseCase.SharedConsentFactory;
using static CO.CDP.OrganisationInformation.Persistence.Forms.SubmissionState;

namespace CO.CDP.Forms.WebApi.Tests.SharedConsent;

public class SharedConsentMapperTest
{
    [Fact]
    public void ItReturnsTheOriginalSharedConsentIfItIsInDraftState()
    {
        var sharedConsent = GivenSharedConsent(state: Draft);

        var mappedSharedConsent = SharedConsentMapper.Map(sharedConsent);

        mappedSharedConsent.Should().Be(sharedConsent);
        mappedSharedConsent.CreatedFrom.Should().BeNull();
    }

    [Fact]
    public void ItCreatesNewSharedConsentIfItIsInSubmittedStateAndThereAreNoAnswersYet()
    {
        var sharedConsent = GivenSharedConsent(state: Submitted);

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
        var formAnswers = new List<FormAnswer>
        {
            GivenAnswer(boolValue: true),
            GivenAnswer(textValue: "Answer 2")
        };
        var answerSet = GivenAnswerSet(sharedConsent, answers: formAnswers);

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
        mappedSharedConsent.AnswerSets.First().Answers.First().Guid.Should().NotBe(formAnswers.First().Guid);
        mappedSharedConsent.AnswerSets.First().Answers.First().QuestionId.Should().Be(formAnswers.First().QuestionId);
        mappedSharedConsent.AnswerSets.First().Answers.First().BoolValue.Should().Be(formAnswers.First().BoolValue);
    }
}