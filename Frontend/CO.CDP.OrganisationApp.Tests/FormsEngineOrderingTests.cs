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
        var answerDisplayServiceMock = new Mock<IAnswerDisplayService>();

        answerDisplayServiceMock.Setup(a => a.FormatAnswerForDisplayAsync(It.IsAny<QuestionAnswer>(), It.IsAny<FormQuestion>()))
            .ReturnsAsync((QuestionAnswer qa, FormQuestion _) => qa.Answer?.TextValue ?? string.Empty);

        _formsEngine = new FormsEngine(formsApiClientMock.Object, _tempDataServiceMock.Object,
            choiceProviderServiceMock.Object, dataSharingClientMock.Object, answerDisplayServiceMock.Object);
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

    [Fact]
    public async Task GetMultiQuestionPage_ShouldOrderQuestionsByJourney_WhenQuestionsHaveNextQuestionChain()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        // Create questions with intentionally mixed order but proper next_question chain
        var titleId = Guid.NewGuid();
        var dateId = Guid.NewGuid();
        var daysId = Guid.NewGuid();
        var labelId = Guid.NewGuid();
        var within30Id = Guid.NewGuid();
        var days31To60Id = Guid.NewGuid();
        var days61PlusId = Guid.NewGuid();
        var overdueId = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "Test Section" },
            Questions = new List<FormQuestion>
            {
                // Questions in mixed order (not journey order)
                new FormQuestion
                {
                    Id = within30Id,
                    Type = FormQuestionType.Text,
                    Title = "Within 30 days (%)",
                    NextQuestion = days31To60Id,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                },
                new FormQuestion
                {
                    Id = titleId,
                    Type = FormQuestionType.NoInput,
                    Title = "Payment Title",
                    NextQuestion = dateId,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                },
                new FormQuestion
                {
                    Id = overdueId,
                    Type = FormQuestionType.Text,
                    Title = "Overdue (%)",
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                },
                new FormQuestion
                {
                    Id = daysId,
                    Type = FormQuestionType.Text,
                    Title = "Average days to pay",
                    NextQuestion = labelId,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                },
                new FormQuestion
                {
                    Id = dateId,
                    Type = FormQuestionType.Date,
                    Title = "Reporting start date",
                    NextQuestion = daysId,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                },
                new FormQuestion
                {
                    Id = days61PlusId,
                    Type = FormQuestionType.Text,
                    Title = "61+ days (%)",
                    NextQuestion = overdueId,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                },
                new FormQuestion
                {
                    Id = labelId,
                    Type = FormQuestionType.NoInput,
                    Title = "Invoices paid:",
                    NextQuestion = within30Id,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                },
                new FormQuestion
                {
                    Id = days31To60Id,
                    Type = FormQuestionType.Text,
                    Title = "31-60 days (%)",
                    NextQuestion = days61PlusId,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                }
            }
        };

        var sessionKey = $"Form_{organisationId}_{formId}_{sectionId}_Questions";
        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionResponse);

        var result = await _formsEngine.GetMultiQuestionPage(organisationId, formId, sectionId, titleId);

        result.Should().NotBeNull();
        result.Questions.Should().HaveCount(8);

        // Verify questions are ordered by journey chain, not database order
        result.Questions[0].Id.Should().Be(titleId, "Title should be first");
        result.Questions[1].Id.Should().Be(dateId, "Date should be second");
        result.Questions[2].Id.Should().Be(daysId, "Days should be third");
        result.Questions[3].Id.Should().Be(labelId, "Label should be fourth");
        result.Questions[4].Id.Should().Be(within30Id, "Within 30 should be fifth");
        result.Questions[5].Id.Should().Be(days31To60Id, "31-60 days should be sixth");
        result.Questions[6].Id.Should().Be(days61PlusId, "61+ days should be seventh");
        result.Questions[7].Id.Should().Be(overdueId, "Overdue should be last");
    }

    [Fact]
    public async Task GetMultiQuestionPage_ShouldHandleBrokenChain_WhenNextQuestionIdMissing()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        var question1Id = Guid.NewGuid();
        var question2Id = Guid.NewGuid();
        var question3Id = Guid.NewGuid();
        var missingQuestionId = Guid.NewGuid(); // This ID won't exist in the questions list

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "Test Section" },
            Questions =
            [
                new FormQuestion
                {
                    Id = question1Id,
                    Type = FormQuestionType.Text,
                    Title = "Question 1",
                    NextQuestion = missingQuestionId, // Points to non-existent question
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                },

                new FormQuestion
                {
                    Id = question2Id,
                    Type = FormQuestionType.Text,
                    Title = "Question 2",
                    NextQuestion = question3Id,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                },

                new FormQuestion
                {
                    Id = question3Id,
                    Type = FormQuestionType.Text,
                    Title = "Question 3",
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                }
            ]
        };

        var sessionKey = $"Form_{organisationId}_{formId}_{sectionId}_Questions";
        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionResponse);

        var result = await _formsEngine.GetMultiQuestionPage(organisationId, formId, sectionId, question1Id);

        result.Should().NotBeNull();
        result.Questions.Should().HaveCount(1, "Should stop at first question when chain is broken");
        result.Questions[0].Id.Should().Be(question1Id);
    }
}