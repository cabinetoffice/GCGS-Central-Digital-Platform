using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using FluentAssertions;
using Moq;
using DataShareWebApiClient = CO.CDP.DataSharing.WebApiClient;
using WebApiClient = CO.CDP.Forms.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests;

public class FormsEngineOrderingTests
{
    private readonly Mock<ITempDataService> _tempDataServiceMock;
    private readonly FormsEngine _formsEngine;

    public FormsEngineOrderingTests()
    {
        var formsApiClientMock = new Mock<WebApiClient.IFormsClient>();
        var dataSharingClientMock = new Mock<DataShareWebApiClient.IDataSharingClient>();
        _tempDataServiceMock = new Mock<ITempDataService>();
        var choiceProviderServiceMock = new Mock<IChoiceProviderService>();
        _formsEngine = new FormsEngine(formsApiClientMock.Object, _tempDataServiceMock.Object,
            choiceProviderServiceMock.Object, dataSharingClientMock.Object);
    }

    [Fact]
    public async Task GetGroupedAnswerSummaries_NoInputMultilineFileUpload_ShouldOrderQuestionsCorrectly()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var noInputQuestionId = Guid.NewGuid();
        var multilineQuestionId = Guid.NewGuid();
        var fileUploadQuestionId = Guid.NewGuid();

        var sectionQuestionsResponse = new SectionQuestionsResponse
        {
            Section = new FormSection
            {
                Id = sectionId,
                Title = "Test Section",
                AllowsMultipleAnswerSets = false
            },
            Questions = new List<FormQuestion>
            {
                new FormQuestion
                {
                    Id = noInputQuestionId,
                    Title = "Information Question",
                    SummaryTitle = "Information Question",
                    Type = FormQuestionType.NoInput,
                    NextQuestion = multilineQuestionId,
                    Options = new FormQuestionOptions()
                },
                new FormQuestion
                {
                    Id = multilineQuestionId,
                    Title = "Multiline Question",
                    SummaryTitle = "Multiline Question",
                    Type = FormQuestionType.MultiLine,
                    NextQuestion = fileUploadQuestionId,
                    Options = new FormQuestionOptions()
                },
                new FormQuestion
                {
                    Id = fileUploadQuestionId,
                    Title = "File Upload Question",
                    SummaryTitle = "File Upload Question",
                    Type = FormQuestionType.FileUpload,
                    IsRequired = false,
                    Options = new FormQuestionOptions()
                }
            }
        };

        var sessionKey = $"Form_{organisationId}_{formId}_{sectionId}_Questions";
        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionQuestionsResponse);

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = multilineQuestionId,
                    Answer = new FormAnswer { TextValue = "This is a multiline answer" }
                },
                new QuestionAnswer
                {
                    QuestionId = fileUploadQuestionId,
                    Answer = new FormAnswer { BoolValue = false }
                }
            }
        };

        var result = await _formsEngine.GetGroupedAnswerSummaries(organisationId, formId, sectionId, answerState);

        result.Should().HaveCount(2, "because NoInput questions are filtered out and we have 2 answered questions");

        var answerSummaries = result.Cast<AnswerSummary>().ToList();

        answerSummaries[0].Title.Should().Be("Multiline Question",
            "because the multiline question should appear before the file upload question in the check your answers page");
        answerSummaries[0].Answer.Should().Be("This is a multiline answer");

        answerSummaries[1].Title.Should().Be("File Upload Question",
            "because the file upload question should appear after the multiline question in the check your answers page");
        answerSummaries[1].Answer.Should().Be("No");
    }

    [Fact]
    public async Task GetGroupedAnswerSummaries_PreservesQuestionSequenceOrder_WhenQuestionsAreDefinedInOrder()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var firstQuestionId = Guid.NewGuid();
        var secondQuestionId = Guid.NewGuid();
        var thirdQuestionId = Guid.NewGuid();

        var sectionQuestionsResponse = new SectionQuestionsResponse
        {
            Section = new FormSection
            {
                Id = sectionId,
                Title = "Test Section",
                AllowsMultipleAnswerSets = false
            },
            Questions = new List<FormQuestion>
            {
                new FormQuestion
                {
                    Id = firstQuestionId,
                    Title = "First Question",
                    SummaryTitle = "First Question",
                    Type = FormQuestionType.Text,
                    NextQuestion = secondQuestionId,
                    Options = new FormQuestionOptions()
                },
                new FormQuestion
                {
                    Id = secondQuestionId,
                    Title = "Second Question",
                    SummaryTitle = "Second Question",
                    Type = FormQuestionType.MultiLine,
                    NextQuestion = thirdQuestionId,
                    Options = new FormQuestionOptions()
                },
                new FormQuestion
                {
                    Id = thirdQuestionId,
                    Title = "Third Question",
                    SummaryTitle = "Third Question",
                    Type = FormQuestionType.FileUpload,
                    IsRequired = false,
                    Options = new FormQuestionOptions()
                }
            }
        };

        var sessionKey = $"Form_{organisationId}_{formId}_{sectionId}_Questions";
        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionQuestionsResponse);

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = firstQuestionId,
                    Answer = new FormAnswer { TextValue = "First answer" }
                },
                new QuestionAnswer
                {
                    QuestionId = secondQuestionId,
                    Answer = new FormAnswer { TextValue = "Second answer" }
                },
                new QuestionAnswer
                {
                    QuestionId = thirdQuestionId,
                    Answer = new FormAnswer { BoolValue = false }
                }
            }
        };

        var result = await _formsEngine.GetGroupedAnswerSummaries(organisationId, formId, sectionId, answerState);

        result.Should().HaveCount(3, "because all 3 questions have answers");

        var answerSummaries = result.Cast<AnswerSummary>().ToList();

        answerSummaries[0].Title.Should().Be("First Question",
            "because the first question should appear first in the check your answers page");
        answerSummaries[0].Answer.Should().Be("First answer");

        answerSummaries[1].Title.Should().Be("Second Question",
            "because the second question should appear second in the check your answers page");
        answerSummaries[1].Answer.Should().Be("Second answer");

        answerSummaries[2].Title.Should().Be("Third Question",
            "because the third question should appear third in the check your answers page");
        answerSummaries[2].Answer.Should().Be("No");
    }

    [Fact]
    public async Task GetGroupedAnswerSummaries_WithScrambledQuestionList_ShouldMaintainCorrectOrder()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var firstQuestionId = Guid.NewGuid();
        var secondQuestionId = Guid.NewGuid();
        var thirdQuestionId = Guid.NewGuid();
        var checkYourAnswersId = Guid.NewGuid();

        var sectionQuestionsResponse = new SectionQuestionsResponse
        {
            Section = new FormSection
            {
                Id = sectionId,
                Title = "Test Section",
                AllowsMultipleAnswerSets = false
            },
            Questions = new List<FormQuestion>
            {
                new FormQuestion
                {
                    Id = thirdQuestionId,
                    Title = "Third Question",
                    SummaryTitle = "Third Question",
                    Type = FormQuestionType.FileUpload,
                    IsRequired = false,
                    NextQuestion = checkYourAnswersId,
                    Options = new FormQuestionOptions()
                },
                new FormQuestion
                {
                    Id = firstQuestionId,
                    Title = "First Question",
                    SummaryTitle = "First Question",
                    Type = FormQuestionType.Text,
                    NextQuestion = secondQuestionId,
                    Options = new FormQuestionOptions()
                },
                new FormQuestion
                {
                    Id = secondQuestionId,
                    Title = "Second Question",
                    SummaryTitle = "Second Question",
                    Type = FormQuestionType.MultiLine,
                    NextQuestion = thirdQuestionId,
                    Options = new FormQuestionOptions()
                },
                new FormQuestion
                {
                    Id = checkYourAnswersId,
                    Title = "Check Your Answers",
                    SummaryTitle = "Check Your Answers",
                    Type = FormQuestionType.CheckYourAnswers,
                    Options = new FormQuestionOptions()
                }
            }
        };

        var sessionKey = $"Form_{organisationId}_{formId}_{sectionId}_Questions";
        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionQuestionsResponse);

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = firstQuestionId,
                    Answer = new FormAnswer { TextValue = "First answer" }
                },
                new QuestionAnswer
                {
                    QuestionId = secondQuestionId,
                    Answer = new FormAnswer { TextValue = "Second answer" }
                },
                new QuestionAnswer
                {
                    QuestionId = thirdQuestionId,
                    Answer = new FormAnswer { BoolValue = false }
                }
            }
        };

        var result = await _formsEngine.GetGroupedAnswerSummaries(organisationId, formId, sectionId, answerState);

        result.Should().HaveCount(3, "because all 3 questions have answers");

        var answerSummaries = result.Cast<AnswerSummary>().ToList();

        answerSummaries[0].Title.Should().Be("First Question",
            "because questions should be ordered by their logical flow, not their position in the list");
        answerSummaries[0].Answer.Should().Be("First answer");

        answerSummaries[1].Title.Should().Be("Second Question",
            "because the second question logically follows the first question");
        answerSummaries[1].Answer.Should().Be("Second answer");

        answerSummaries[2].Title.Should().Be("Third Question",
            "because the third question logically follows the second question");
        answerSummaries[2].Answer.Should().Be("No");
    }
}