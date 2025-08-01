using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.Forms.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using FluentAssertions;
using Moq;
using FormAnswer = CO.CDP.OrganisationApp.Models.FormAnswer;
using FormQuestion = CO.CDP.OrganisationApp.Models.FormQuestion;
using FormQuestionGrouping = CO.CDP.OrganisationApp.Models.FormQuestionGrouping;
using FormQuestionOptions = CO.CDP.OrganisationApp.Models.FormQuestionOptions;
using FormQuestionType = CO.CDP.OrganisationApp.Models.FormQuestionType;
using FormSection = CO.CDP.OrganisationApp.Models.FormSection;
using SectionQuestionsResponse = CO.CDP.OrganisationApp.Models.SectionQuestionsResponse;

namespace CO.CDP.OrganisationApp.Tests;

public class FormsEngineMultipleQuestionPageTests
{
    private readonly Mock<ITempDataService> _tempDataServiceMock;
    private readonly Mock<IChoiceProviderService> _choiceProviderServiceMock;
    public Mock<IUserInfoService> UserInfoServiceMock;
    public Mock<IOrganisationClient> OrganisationClientMock;
    private readonly FormsEngine _formsEngine;

    public FormsEngineMultipleQuestionPageTests()
    {
        var formsApiClientMock = new Mock<IFormsClient>();
        var dataSharingClientMock = new Mock<IDataSharingClient>();
        _tempDataServiceMock = new Mock<ITempDataService>();
        _choiceProviderServiceMock = new Mock<IChoiceProviderService>();
        UserInfoServiceMock = new Mock<IUserInfoService>();
        OrganisationClientMock = new Mock<IOrganisationClient>();

        _formsEngine = new FormsEngine(formsApiClientMock.Object, _tempDataServiceMock.Object,
            _choiceProviderServiceMock.Object, dataSharingClientMock.Object);
    }

    private static (Guid organisationId, Guid formId, Guid sectionId, string sessionKey) CreateTestGuids()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var sessionKey = $"Form_{organisationId}_{formId}_{sectionId}_Questions";
        return (organisationId, formId, sectionId, sessionKey);
    }

    [Fact]
    public async Task GetMultiQuestionPage_ShouldReturnMultipleQuestions_WhenMultiQuestionConfigurationExists()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var firstQuestionId = Guid.NewGuid();
        var secondQuestionId = Guid.NewGuid();
        var thirdQuestionId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions =
            [
                new FormQuestion
                {
                    Id = firstQuestionId,
                    Type = FormQuestionType.Text,
                    Title = "Question 1",
                    NextQuestion = secondQuestionId,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping
                        {
                            Id = groupId,
                            Page = true,
                            CheckYourAnswers = false,
                            SummaryTitle = "ModernSlavery_02_Title"
                        },
                    },
                },

                new FormQuestion
                {
                    Id = secondQuestionId,
                    Type = FormQuestionType.YesOrNo,
                    Title = "Question 2",
                    NextQuestion = thirdQuestionId,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping
                        {
                            Id = groupId,
                            Page = true,
                            CheckYourAnswers = false,
                            SummaryTitle = "ModernSlavery_02_Title"
                        }
                    },
                },

                new FormQuestion
                {
                    Id = thirdQuestionId,
                    Type = FormQuestionType.Date,
                    Title = "Question 3",
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping
                        {
                            Id = groupId,
                            Page = true,
                            CheckYourAnswers = false,
                            SummaryTitle = "ModernSlavery_02_Title"
                        }
                    }
                }
            ]
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionResponse);

        var result = await _formsEngine.GetMultiQuestionPage(organisationId, formId, sectionId, firstQuestionId);

        result.Should().NotBeNull();
        result.Questions.Should().HaveCount(3);
        result.Questions[0].Id.Should().Be(firstQuestionId);
        result.Questions[1].Id.Should().Be(secondQuestionId);
        result.Questions[2].Id.Should().Be(thirdQuestionId);
    }

    [Fact]
    public async Task GetMultiQuestionPage_ShouldReturnSingleQuestion_WhenNoMultiQuestionConfiguration()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var questionId = Guid.NewGuid();
        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions =
            [
                new FormQuestion
                {
                    Id = questionId,
                    Type = FormQuestionType.Text,
                    Title = "Single Question",
                    Options = new FormQuestionOptions()
                }
            ]
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionResponse);

        var result = await _formsEngine.GetMultiQuestionPage(organisationId, formId, sectionId, questionId);

        result.Should().NotBeNull();
        result.Questions.Should().HaveCount(1);
        result.Questions[0].Id.Should().Be(questionId);
    }

    [Fact]
    public async Task GetNextQuestion_ShouldSkipQuestionsInMultiQuestionPage_WhenMultiQuestionConfigurationExists()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var groupId = Guid.NewGuid();
        var firstQuestionId = Guid.NewGuid();
        var secondQuestionId = Guid.NewGuid();
        var thirdQuestionId = Guid.NewGuid();
        var fourthQuestionId = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions =
            {
                new FormQuestion
                {
                    Id = firstQuestionId,
                    NextQuestion = secondQuestionId,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping
                        {
                            Id = groupId,
                            Page = true,
                            CheckYourAnswers = false,
                            SummaryTitle = "MultiQuestionPage"
                        }
                    }
                },
                new FormQuestion
                {
                    Id = secondQuestionId,
                    NextQuestion = fourthQuestionId,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping
                        {
                            Id = groupId,
                            Page = true,
                            CheckYourAnswers = false,
                            SummaryTitle = "MultiQuestionPage"
                        }
                    }
                },
                new FormQuestion { Id = thirdQuestionId, NextQuestion = fourthQuestionId },
                new FormQuestion { Id = fourthQuestionId }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionResponse);

        var answerState = new FormQuestionAnswerState();
        var result = await _formsEngine.GetNextQuestion(organisationId, formId, sectionId, firstQuestionId, answerState);

        result.Should().NotBeNull();
        result!.Id.Should().Be(fourthQuestionId,
            "because the next question should skip the multi-question page questions");
    }

    [Fact]
    public async Task GetGroupedAnswerSummaries_ShouldReturnGroupedAnswers_WhenMultiQuestionConfiguration()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var groupId = Guid.NewGuid();
        var question1Id = Guid.NewGuid();
        var question2Id = Guid.NewGuid();
        var question3Id = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "Test Section" },
            Questions =
            {
                new FormQuestion
                {
                    Id = question1Id,
                    Type = FormQuestionType.Text,
                    Title = "Question 1",
                    NextQuestion = question2Id,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping
                        {
                            Id = groupId,
                            Page = true,
                            CheckYourAnswers = true,
                            SummaryTitle = "Test Group Title"
                        }
                    }
                },
                new FormQuestion
                {
                    Id = question2Id,
                    Type = FormQuestionType.YesOrNo,
                    Title = "Question 2",
                    NextQuestion = question3Id,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping
                        {
                            Id = groupId,
                            Page = true,
                            CheckYourAnswers = true,
                            SummaryTitle = "Test Group Title"
                        }
                    }
                },
                new FormQuestion
                {
                    Id = question3Id,
                    Type = FormQuestionType.Date,
                    Title = "Question 3",
                    Options = new FormQuestionOptions()
                }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers =
            {
                new QuestionAnswer { QuestionId = question1Id, Answer = new FormAnswer { TextValue = "Answer 1" } },
                new QuestionAnswer { QuestionId = question2Id, Answer = new FormAnswer { BoolValue = false } },
                new QuestionAnswer { QuestionId = question3Id, Answer = new FormAnswer { DateValue = DateTimeOffset.Parse("2024-01-01") } }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionResponse);

        _choiceProviderServiceMock.Setup(c => c.GetStrategy(null))
            .Returns(new DefaultChoiceProviderStrategy());

        var result = await _formsEngine.GetGroupedAnswerSummaries(organisationId, formId, sectionId, answerState);

        result.Should().HaveCount(2, "because the multi-question configuration should group the first 2 questions");

        var groupedAnswer = result.First() as GroupedAnswerSummary;
        groupedAnswer.Should().NotBeNull();
        groupedAnswer!.IsGroup.Should().BeTrue();
        groupedAnswer.GroupTitle.Should().Be("Test Group Title");
        groupedAnswer.GroupChangeLink.Should()
            .Be($"/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{question1Id}");
        groupedAnswer.Answers.Should().HaveCount(2);
        groupedAnswer.Answers[0].Title.Should().Be("Question 1");
        groupedAnswer.Answers[0].Answer.Should().Be("Answer 1");
        groupedAnswer.Answers[1].Title.Should().Be("Question 2");
        groupedAnswer.Answers[1].Answer.Should().Be("No");
    }

    [Fact]
    public async Task GetGroupedAnswerSummaries_ShouldMixGroupedAndIndividualAnswers()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var groupId = Guid.NewGuid();
        var question1Id = Guid.NewGuid();
        var question2Id = Guid.NewGuid();
        var question3Id = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "Test Section" },
            Questions =
            {
                new FormQuestion { Id = question1Id, Type = FormQuestionType.Text, Title = "Grouped Question 1", NextQuestion = question2Id, Options = new FormQuestionOptions { Grouping = new FormQuestionGrouping { Id = groupId, Page = true, CheckYourAnswers = true, SummaryTitle = "Group Title" } } },
                new FormQuestion { Id = question2Id, Type = FormQuestionType.YesOrNo, Title = "Grouped Question 2", NextQuestion = question3Id, Options = new FormQuestionOptions { Grouping = new FormQuestionGrouping { Id = groupId, Page = true, CheckYourAnswers = true, SummaryTitle = "Group Title" } } },
                new FormQuestion { Id = question3Id, Type = FormQuestionType.Text, Title = "Individual Question", Options = new FormQuestionOptions() }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers =
            {
                new QuestionAnswer { QuestionId = question1Id, Answer = new FormAnswer { TextValue = "Grouped Answer 1" } },
                new QuestionAnswer { QuestionId = question2Id, Answer = new FormAnswer { BoolValue = true } },
                new QuestionAnswer { QuestionId = question3Id, Answer = new FormAnswer { TextValue = "Individual Answer" } }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionResponse);

        _choiceProviderServiceMock.Setup(c => c.GetStrategy(null))
            .Returns(new DefaultChoiceProviderStrategy());

        var result = await _formsEngine.GetGroupedAnswerSummaries(organisationId, formId, sectionId, answerState);

        result.Should().HaveCount(2);
        var groupedAnswer = result.FirstOrDefault(r => r.IsGroup) as GroupedAnswerSummary;
        groupedAnswer.Should().NotBeNull();
        groupedAnswer!.GroupTitle.Should().Be("Group Title");
        groupedAnswer.Answers.Should().HaveCount(2);

        var individualAnswer = result.FirstOrDefault(r => !r.IsGroup) as AnswerSummary;
        individualAnswer.Should().NotBeNull();
        individualAnswer!.Title.Should().Be("Individual Question");
    }

    [Fact]
    public async Task GetGroupedAnswerSummaries_ShouldSkipUnansweredQuestions()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var question1Id = Guid.NewGuid();
        var question2Id = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "Test Section" },
            Questions =
            {
                new FormQuestion { Id = question1Id, Type = FormQuestionType.Text, Title = "Answered Question", Options = new FormQuestionOptions() },
                new FormQuestion { Id = question2Id, Type = FormQuestionType.Text, Title = "Unanswered Question", Options = new FormQuestionOptions() }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = { new QuestionAnswer { QuestionId = question1Id, Answer = new FormAnswer { TextValue = "I have an answer" } } }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionResponse);

        _choiceProviderServiceMock.Setup(c => c.GetStrategy(null))
            .Returns(new DefaultChoiceProviderStrategy());

        var result = await _formsEngine.GetGroupedAnswerSummaries(organisationId, formId, sectionId, answerState);

        result.Should().HaveCount(1);
        var answer = result.First() as AnswerSummary;
        answer!.Title.Should().Be("Answered Question");
    }

    [Fact]
    public async Task GetNextQuestion_ShouldNavigateToExitQuestion_WhenSubmittingFromMultiQuestionPage()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var groupId = Guid.NewGuid();

        var textQuestionId = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var yesNoQuestionId = new Guid("99999999-9999-9999-9999-999999999999");
        var fileUploadQuestionId = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd");

        var groupStartQuestionId = new Guid("a2222222-2222-2222-2222-222222222222");

        var checkYourAnswersId = new Guid("a1111111-1111-1111-1111-111111111111");

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "Test Section" },
            Questions =
            {
                new FormQuestion
                {
                    Id = textQuestionId,
                    Type = FormQuestionType.Text,
                    Title = "Text Question",
                    NextQuestion = yesNoQuestionId,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping
                        {
                            Id = groupId,
                            Page = true,
                            CheckYourAnswers = true,
                            SummaryTitle = "Multi Question Group"
                        }
                    }
                },
                new FormQuestion
                {
                    Id = yesNoQuestionId,
                    Type = FormQuestionType.YesOrNo,
                    Title = "Yes/No Question",
                    NextQuestion = fileUploadQuestionId,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping
                        {
                            Id = groupId,
                            Page = true,
                            CheckYourAnswers = true,
                            SummaryTitle = "Multi Question Group"
                        }
                    }
                },
                new FormQuestion
                {
                    Id = fileUploadQuestionId,
                    Type = FormQuestionType.FileUpload,
                    Title = "File Upload Question",
                    NextQuestion = groupStartQuestionId,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping
                        {
                            Id = groupId,
                            Page = true,
                            CheckYourAnswers = true,
                            SummaryTitle = "Multi Question Group"
                        }
                    }
                },
                new FormQuestion
                {
                    Id = groupStartQuestionId,
                    Type = FormQuestionType.Text,
                    Title = "Group Start Question",
                    NextQuestion = checkYourAnswersId,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping
                        {
                            Id = groupId,
                            Page = true,
                            CheckYourAnswers = true,
                            SummaryTitle = "Multi Question Group"
                        }
                    }
                },
                new FormQuestion
                {
                    Id = checkYourAnswersId,
                    Type = FormQuestionType.CheckYourAnswers,
                    Title = "Check Your Answers",
                    Options = new FormQuestionOptions()
                }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers =
            {
                new QuestionAnswer { QuestionId = textQuestionId, Answer = new FormAnswer { TextValue = "Text answer" } },
                new QuestionAnswer { QuestionId = yesNoQuestionId, Answer = new FormAnswer { BoolValue = true } },
                new QuestionAnswer { QuestionId = fileUploadQuestionId, Answer = new FormAnswer { TextValue = "file.pdf" } }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionResponse);

        var resultFromTextQuestion = await _formsEngine.GetNextQuestion(organisationId, formId, sectionId, textQuestionId, answerState);
        var resultFromYesNoQuestion = await _formsEngine.GetNextQuestion(organisationId, formId, sectionId, yesNoQuestionId, answerState);
        var resultFromFileUploadQuestion = await _formsEngine.GetNextQuestion(organisationId, formId, sectionId, fileUploadQuestionId, answerState);

        resultFromTextQuestion.Should().NotBeNull();
        resultFromTextQuestion!.Id.Should().Be(checkYourAnswersId,
            "navigation from text question in multi-question page should go to Check Your Answers");

        resultFromYesNoQuestion.Should().NotBeNull();
        resultFromYesNoQuestion!.Id.Should().Be(checkYourAnswersId,
            "navigation from yes/no question in multi-question page should go to Check Your Answers");

        resultFromFileUploadQuestion.Should().NotBeNull();
        resultFromFileUploadQuestion!.Id.Should().Be(checkYourAnswersId,
            "navigation from file upload question in multi-question page should go to Check Your Answers");
    }

    [Fact]
    public async Task GetGroupedAnswerSummaries_WhenGroupedOnPage_ShouldHaveSingleChangeLink()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var groupId = Guid.NewGuid();
        var question1Id = Guid.NewGuid();
        var question2Id = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "Test Section" },
            Questions =
            {
                new FormQuestion
                {
                    Id = question1Id, Type = FormQuestionType.Text, Title = "Grouped Question 1",
                    NextQuestion = question2Id,
                    Options = new FormQuestionOptions
                        { Grouping = new FormQuestionGrouping { Id = groupId, Page = true, CheckYourAnswers = true, SummaryTitle = "Group Title" } }
                },
                new FormQuestion
                {
                    Id = question2Id, Type = FormQuestionType.YesOrNo, Title = "Grouped Question 2",
                    Options = new FormQuestionOptions
                        { Grouping = new FormQuestionGrouping { Id = groupId, Page = true, CheckYourAnswers = true, SummaryTitle = "Group Title" } }
                }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers =
            {
                new QuestionAnswer { QuestionId = question1Id, Answer = new FormAnswer { TextValue = "Grouped Answer 1" } },
                new QuestionAnswer { QuestionId = question2Id, Answer = new FormAnswer { BoolValue = true } }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey)).Returns(sectionResponse);
        _choiceProviderServiceMock.Setup(c => c.GetStrategy(null)).Returns(new DefaultChoiceProviderStrategy());

        var result = await _formsEngine.GetGroupedAnswerSummaries(organisationId, formId, sectionId, answerState);

        var groupedAnswer = result.First() as GroupedAnswerSummary;
        groupedAnswer.Should().NotBeNull();
        groupedAnswer!.GroupChangeLink.Should().NotBeNull();
        groupedAnswer.Answers.ForEach(a => a.ChangeLink.Should().Contain("?frm-chk-answer=true"));
    }

    [Fact]
    public async Task GetGroupedAnswerSummaries_WhenGroupedOnSummaryOnly_ShouldHaveIndividualChangeLinks()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var groupId = Guid.NewGuid();
        var question1Id = Guid.NewGuid();
        var question2Id = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "Test Section" },
            Questions =
            {
                new FormQuestion
                {
                    Id = question1Id, Type = FormQuestionType.Text, Title = "Grouped Question 1",
                    NextQuestion = question2Id,
                    Options = new FormQuestionOptions
                        { Grouping = new FormQuestionGrouping { Id = groupId, Page = false, CheckYourAnswers = true, SummaryTitle = "Group Title" } }
                },
                new FormQuestion
                {
                    Id = question2Id, Type = FormQuestionType.YesOrNo, Title = "Grouped Question 2",
                    Options = new FormQuestionOptions
                        { Grouping = new FormQuestionGrouping { Id = groupId, Page = false, CheckYourAnswers = true, SummaryTitle = "Group Title" } }
                }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers =
            {
                new QuestionAnswer { QuestionId = question1Id, Answer = new FormAnswer { TextValue = "Grouped Answer 1" } },
                new QuestionAnswer { QuestionId = question2Id, Answer = new FormAnswer { BoolValue = true } }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey)).Returns(sectionResponse);
        _choiceProviderServiceMock.Setup(c => c.GetStrategy(null)).Returns(new DefaultChoiceProviderStrategy());

        var result = await _formsEngine.GetGroupedAnswerSummaries(organisationId, formId, sectionId, answerState);

        var groupedAnswer = result.First() as GroupedAnswerSummary;
        groupedAnswer.Should().NotBeNull();
        groupedAnswer!.GroupChangeLink.Should().BeNull();
        groupedAnswer.Answers[0].ChangeLink.Should().Be($"/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{question1Id}?frm-chk-answer=true");
        groupedAnswer.Answers[1].ChangeLink.Should().Be($"/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{question2Id}?frm-chk-answer=true");
    }

    [Fact]
    public async Task GetMultiQuestionPage_ShouldStartFromCorrectQuestion_WhenNavigatingToMidGroupQuestion()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var groupId = Guid.NewGuid();
        
        var titleId = Guid.NewGuid();
        var question1Id = Guid.NewGuid();
        var question2Id = Guid.NewGuid();
        var question3Id = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "Test Section" },
            Questions =
            [
                new FormQuestion
                {
                    Id = titleId,
                    Type = FormQuestionType.NoInput,
                    Title = "Group Title",
                    NextQuestion = question1Id,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                },
                new FormQuestion
                {
                    Id = question1Id,
                    Type = FormQuestionType.Text,
                    Title = "Question 1",
                    NextQuestion = question2Id,
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

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionResponse);

        // Navigate to a question in the middle of the group
        var result = await _formsEngine.GetMultiQuestionPage(organisationId, formId, sectionId, question2Id);

        result.Should().NotBeNull();
        result.Questions.Should().HaveCount(2, "Should only return questions from the starting point onwards");
        result.Questions[0].Id.Should().Be(question2Id, "Should start from the specified question");
        result.Questions[1].Id.Should().Be(question3Id, "Should continue the chain from starting question");
    }

    [Fact]
    public async Task GetPreviousQuestion_ShouldReturnGroupStart_WhenCurrentQuestionIsInMiddleOfGroup()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var groupId = Guid.NewGuid();
        
        var beforeGroupId = Guid.NewGuid();
        var titleId = Guid.NewGuid();
        var question1Id = Guid.NewGuid();
        var question2Id = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "Test Section" },
            Questions =
            [
                new FormQuestion
                {
                    Id = beforeGroupId,
                    Type = FormQuestionType.Text,
                    Title = "Before Group",
                    NextQuestion = titleId,
                    Options = new FormQuestionOptions()
                },
                new FormQuestion
                {
                    Id = titleId,
                    Type = FormQuestionType.NoInput,
                    Title = "Group Title",
                    NextQuestion = question1Id,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                },
                new FormQuestion
                {
                    Id = question1Id,
                    Type = FormQuestionType.Text,
                    Title = "Question 1",
                    NextQuestion = question2Id,
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
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                }
            ]
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionResponse);

        var answerState = new FormQuestionAnswerState
        {
            Answers =
            [
                new QuestionAnswer { QuestionId = beforeGroupId, Answer = new FormAnswer { TextValue = "Before answer" } }
            ]
        };

        // When navigating to a question in the middle of a group, 
        // previous question should return the question before the group start
        var result = await _formsEngine.GetPreviousQuestion(organisationId, formId, sectionId, question2Id, answerState);

        result.Should().NotBeNull();
        result!.Id.Should().Be(beforeGroupId, "Previous question should be the question before the group starts");
    }

    [Fact]
    public async Task CollectQuestionsForPage_ShouldHandlePartialChain_WhenStartingFromMiddleQuestion()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var groupId = Guid.NewGuid();
        
        var question1Id = Guid.NewGuid();
        var question2Id = Guid.NewGuid();
        var question3Id = Guid.NewGuid();
        var question4Id = Guid.NewGuid();

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
                    NextQuestion = question2Id,
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
                    NextQuestion = question4Id,
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                },
                new FormQuestion
                {
                    Id = question4Id,
                    Type = FormQuestionType.Text,
                    Title = "Question 4",
                    Options = new FormQuestionOptions
                    {
                        Grouping = new FormQuestionGrouping { Id = groupId, Page = true }
                    }
                }
            ]
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(sectionResponse);

        // Start from question 3 (middle of the chain)
        var result = await _formsEngine.GetMultiQuestionPage(organisationId, formId, sectionId, question3Id);

        result.Should().NotBeNull();
        result.Questions.Should().HaveCount(2, "Should return only questions from starting point onwards");
        result.Questions[0].Id.Should().Be(question3Id);
        result.Questions[1].Id.Should().Be(question4Id);
    }

}
