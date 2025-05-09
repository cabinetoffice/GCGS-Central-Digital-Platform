using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Forms.AnswerSummaryStrategies;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using FluentAssertions;
using Moq;
namespace CO.CDP.OrganisationApp.Tests.Pages.Forms.AnswerSummaryStrategies;

public class FormatterAnswerSummaryStrategyTests
{
    [Fact]
    public async Task GetAnswerSummaries_ShouldReturnFormattedSummary()
    {
        var keyExpr = "\"yes\"==\"yes\"?\"FormattedKey\":\"Other\"";
        var valExpr = "Summary: {0}";

        var questionId = Guid.NewGuid();
        List<FormQuestion> questions = CreateFormQuestions();
        FormAnswerSet answerSet = CreateFormAnswerSet(questions);

        var answerSets = new List<FormAnswerSet>
        {
            answerSet
        };

        FormSection formSection = CreateFormSection(keyExpr, valExpr);

        var form = new SectionQuestionsResponse(answerSets, questions, formSection);

        var evaluators = new IEvaluator[] { new StringFormatEvaluator(), new EqualityEvaluator() };
        var factory = new EvaluatorFactory(evaluators);
        
        var choiceProviderMock = new Mock<IChoiceProviderService>();

        var strategy = new FormatterAnswerSummaryStrategy(factory, choiceProviderMock.Object);

        var result = await strategy.GetAnswerSummaries(form, answerSet);

        result.Should().NotBeNull();
        result.Should().ContainSingle();
        result[0].Title.Should().Be("FormattedKey");
        result[0].Answer.Should().Be("Summary: Answer 1");
    }

    private static FormSection CreateFormSection(string keyExpr, string valExpr)
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
                             summaryRenderFormatter: new SummaryRenderFormatter(
                                                    keyExpression: keyExpr,
                                                    keyExpressionOperation: ExpressionOperationType.Equality,
                                                    keyParams: [],
                                                    valueExpression: valExpr,
                                                    valueExpressionOperation: ExpressionOperationType.StringFormat,
                                                    valueParams: new[] { "question1" })));
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
                            textValue: "Answer 1")
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
                    name: "question1",
                    nextQuestion: null,
                    nextQuestionAlternative: null,
                    options: null,
                    summaryTitle: "Summary 1",
                    title: "Question 1",
                    type: FormQuestionType.Text)
            };
    }    
}
