using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Forms.AnswerSummaryStrategies;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using FluentAssertions;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms.AnswerSummaryStrategies;

public class DefaultAnswerSummaryStrategyTests
{
    private readonly Mock<IChoiceProviderService> _mockChoiceProviderService;
    private readonly DefaultAnswerSummaryStrategy _strategy;

    public DefaultAnswerSummaryStrategyTests()
    {
        _mockChoiceProviderService = new Mock<IChoiceProviderService>();
        _strategy = new DefaultAnswerSummaryStrategy(_mockChoiceProviderService.Object);
    }

    [Fact]
    public async Task GetAnswerSummaries_ShouldReturnEmpty_WhenNoValidQuestions()
    {
        List<FormQuestion> questions = CreateFormQuestions();
        FormAnswerSet answerSet = CreateFormAnswerSet(questions);

        var answerSets = new List<FormAnswerSet>();
        
        FormSection formSection = CreateFormSection();
        var form = new SectionQuestionsResponse([], [], formSection);

        var result = await _strategy.GetAnswerSummaries(form, answerSet);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAnswerSummaries_ShouldReturnCorrectSummaries()
    {
        List<FormQuestion> questions = CreateFormQuestions();
        FormAnswerSet answerSet = CreateFormAnswerSet(questions);

        var answerSets = new List<FormAnswerSet>
        {
            answerSet
        };

        FormSection formSection = CreateFormSection();

        var form = new SectionQuestionsResponse(answerSets, questions, formSection);

        _mockChoiceProviderService
            .Setup(t => t.GetStrategy(It.IsAny<string>()))
            .Returns(new DefaultChoiceProviderStrategy());

        var result = await _strategy.GetAnswerSummaries(form, answerSet);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Summary 1");
        result[0].Answer.Should().Be("Answer 1");
        result[1].Title.Should().Be("Summary 2");
        result[1].Answer.Should().Be("31 December 2024");
    }

    private static FormSection CreateFormSection()
    {
        return new FormSection(
                    id: Guid.NewGuid(),
                    title: "Section 1",
                    type: FormSectionType.Standard,
                    allowsMultipleAnswerSets: true,
                    checkFurtherQuestionsExempted: false,
                    configuration: new FormSectionConfiguration(
                        addAnotherAnswerLabel: null,
                             pluralSummaryHeadingFormat: null,
                             removeConfirmationCaption: "Test Caption",
                             removeConfirmationHeading: "Test confimration heading",
                             singularSummaryHeading: null,
                             furtherQuestionsExemptedHeading: null,
                             furtherQuestionsExemptedHint: null,
                             pluralSummaryHeadingHintFormat: null,
                             singularSummaryHeadingHint: null,
                             summaryRenderFormatter: null));
    }
    private static FormAnswerSet CreateFormAnswerSet(List<FormQuestion> questions)
    {
        return new FormAnswerSet(
                    answers: new List<FormAnswer>
                    {
                        new FormAnswer(
                            addressValue: null,
                            boolValue: null,
                            dateValue: null,
                            endValue: null,
                            id: Guid.NewGuid(),
                            jsonValue: null,
                            numericValue: null,
                            optionValue: "Option 1",
                            questionId: questions[0].Id,
                            startValue: null,
                            textValue: "Answer 1"),
                        new FormAnswer(
                            addressValue: null,
                            boolValue: null,
                            dateValue: new DateTime(2024, 12, 31),
                            endValue: null,
                            id: Guid.NewGuid(),
                            jsonValue: null,
                            numericValue: null,
                            optionValue: "Option 2",
                            questionId: questions[1].Id,
                            startValue: null,
                            textValue: $"{DateTime.UtcNow.ToString("dd MMMM yyyy")}")
                    }, furtherQuestionsExempted: false, id: Guid.NewGuid());
    }
    private static List<FormQuestion> CreateFormQuestions()
    {
        return new List<FormQuestion>
            {
                new FormQuestion(
                    caption: "Page caption",
                    description: "Description",
                    id: Guid.NewGuid(),
                    isRequired: true,
                    name: "Question 1",
                    nextQuestion: null,
                    nextQuestionAlternative: null,
                    options: null,
                    summaryTitle: "Summary 1",
                    title: "Question 1",
                    type: FormQuestionType.Text),
                new FormQuestion(
                   caption: "Page caption",
                    description: "Description",
                    id: Guid.NewGuid(),
                    isRequired: true,
                    name: "Question 2",
                    nextQuestion: null,
                    nextQuestionAlternative: null,
                    options: null,
                    summaryTitle: "Summary 2",
                    type: FormQuestionType.Date,
                    title: "Question 2"
                )
            };
    }    
}
