using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using Microsoft.Extensions.Localization;
using Moq;

namespace CO.CDP.OrganisationApp.Tests;

public class AnswerDisplayServiceTests
{
    private readonly Mock<IStringLocalizer<StaticTextResource>> _localizerMock;
    private readonly Mock<IChoiceProviderService> _choiceProviderServiceMock;
    private readonly Mock<IChoiceProviderStrategy> _choiceProviderStrategyMock;
    private readonly AnswerDisplayService _service;

    public AnswerDisplayServiceTests()
    {
        _localizerMock = new Mock<IStringLocalizer<StaticTextResource>>();
        _choiceProviderServiceMock = new Mock<IChoiceProviderService>();
        _choiceProviderStrategyMock = new Mock<IChoiceProviderStrategy>();

        _service = new AnswerDisplayService(_localizerMock.Object, _choiceProviderServiceMock.Object);

        SetupLocalizer();
    }

    private void SetupLocalizer()
    {
        _localizerMock.Setup(l => l[nameof(StaticTextResource.Global_Yes)])
            .Returns(new LocalizedString(nameof(StaticTextResource.Global_Yes), "Yes"));
        _localizerMock.Setup(l => l[nameof(StaticTextResource.Global_No)])
            .Returns(new LocalizedString(nameof(StaticTextResource.Global_No), "No"));
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithNullAnswer_ReturnsEmptyString()
    {
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = null
        };
        var question = new FormQuestion { Type = FormQuestionType.Text };

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("", result);
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithBoolTrueValue_ReturnsYes()
    {
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { BoolValue = true }
        };
        var question = new FormQuestion { Type = FormQuestionType.YesOrNo };

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("Yes", result);
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithBoolFalseValue_ReturnsNo()
    {
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { BoolValue = false }
        };
        var question = new FormQuestion { Type = FormQuestionType.YesOrNo };

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("No", result);
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithTextValue_ReturnsTextValue()
    {
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { TextValue = "Test text answer" }
        };
        var question = new FormQuestion { Type = FormQuestionType.Text };

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("Test text answer", result);
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithMultiLineValue_ReturnsTextValue()
    {
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { TextValue = "Line 1\nLine 2" }
        };
        var question = new FormQuestion { Type = FormQuestionType.MultiLine };

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("Line 1\nLine 2", result);
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithFileUploadValue_ReturnsTextValue()
    {
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { TextValue = "uploaded-file.pdf" }
        };
        var question = new FormQuestion { Type = FormQuestionType.FileUpload };

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("uploaded-file.pdf", result);
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithUrlValue_ReturnsTextValue()
    {
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { TextValue = "https://example.com" }
        };
        var question = new FormQuestion { Type = FormQuestionType.Url };

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("https://example.com", result);
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithDateValue_ReturnsFormattedDate()
    {
        var dateValue = new DateTimeOffset(2023, 12, 25, 0, 0, 0, TimeSpan.Zero);
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { DateValue = dateValue }
        };
        var question = new FormQuestion { Type = FormQuestionType.Date };

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("25 December 2023", result);
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithCheckBoxTrueAndChoices_ReturnsCombinedYesAndChoice()
    {
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { BoolValue = true }
        };
        var question = new FormQuestion
        {
            Type = FormQuestionType.CheckBox,
            Options = new FormQuestionOptions
            {
                Choices = new Dictionary<string, string> { { "key1", "Checkbox label" } }
            }
        };

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("Yes, Checkbox label", result);
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithCheckBoxFalse_ReturnsNo()
    {
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { BoolValue = false }
        };
        var question = new FormQuestion
        {
            Type = FormQuestionType.CheckBox,
            Options = new FormQuestionOptions
            {
                Choices = new Dictionary<string, string> { { "key1", "Checkbox label" } }
            }
        };

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("No", result);
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithAddressValue_ReturnsFormattedAddress()
    {
        var address = new Address
        {
            AddressLine1 = "123 Test Street",
            TownOrCity = "Test City",
            Postcode = "TE1 1ST",
            CountryName = "United Kingdom",
            Country = "GB"
        };
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { AddressValue = address }
        };
        var question = new FormQuestion { Type = FormQuestionType.Address };

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("123 Test Street, Test City, TE1 1ST, United Kingdom", result);
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithSingleChoice_CallsChoiceProviderStrategy()
    {
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { OptionValue = "option1" }
        };
        var question = new FormQuestion
        {
            Type = FormQuestionType.SingleChoice,
            Options = new FormQuestionOptions { ChoiceProviderStrategy = "TestStrategy" }
        };

        _choiceProviderServiceMock.Setup(s => s.GetStrategy("TestStrategy"))
            .Returns(_choiceProviderStrategyMock.Object);
        _choiceProviderStrategyMock.Setup(s => s.RenderOption(It.IsAny<FormAnswer>()))
            .ReturnsAsync("Rendered Option");

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("Rendered Option", result);
        _choiceProviderServiceMock.Verify(s => s.GetStrategy("TestStrategy"), Times.Once);
        _choiceProviderStrategyMock.Verify(s => s.RenderOption(questionAnswer.Answer), Times.Once);
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithGroupedSingleChoice_ReturnsMatchingChoiceTitle()
    {
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { OptionValue = "value2" }
        };
        var question = new FormQuestion
        {
            Type = FormQuestionType.GroupedSingleChoice,
            Options = new FormQuestionOptions
            {
                Groups = new List<FormQuestionGroup>
                {
                    new FormQuestionGroup
                    {
                        Name = "Group 1",
                        Choices = new List<FormQuestionGroupChoice>
                        {
                            new FormQuestionGroupChoice { Title = "Choice 1", Value = "value1" },
                            new FormQuestionGroupChoice { Title = "Choice 2", Value = "value2" }
                        }
                    }
                }
            }
        };

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("Choice 2", result);
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithGroupedSingleChoiceNoMatch_ReturnsOptionValue()
    {
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { OptionValue = "unknown_value" }
        };
        var question = new FormQuestion
        {
            Type = FormQuestionType.GroupedSingleChoice,
            Options = new FormQuestionOptions
            {
                Groups = new List<FormQuestionGroup>
                {
                    new FormQuestionGroup
                    {
                        Name = "Group 1",
                        Choices = new List<FormQuestionGroupChoice>
                        {
                            new FormQuestionGroupChoice { Title = "Choice 1", Value = "value1" }
                        }
                    }
                }
            }
        };

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("unknown_value", result);
    }

    [Fact]
    public async Task FormatAnswerForDisplayAsync_WithBoolAndTextValues_CombinesWithComma()
    {
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = Guid.NewGuid(),
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer
            {
                BoolValue = true,
                TextValue = "Additional text"
            }
        };
        var question = new FormQuestion { Type = FormQuestionType.Text };

        var result = await _service.FormatAnswerForDisplayAsync(questionAnswer, question);

        Assert.Equal("Yes, Additional text", result);
    }

    [Fact]
    public void CreateAnswerSummary_WithBasicQuestion_CreatesCorrectSummary()
    {
        var questionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var question = new FormQuestion
        {
            Id = questionId,
            Title = "Test Question",
            Type = FormQuestionType.Text
        };
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = questionId,
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { TextValue = "Test Answer" }
        };

        var result = _service.CreateAnswerSummary(question, questionAnswer, "Test Answer", organisationId, formId, sectionId);

        Assert.Equal("Test Question", result.Title);
        Assert.Equal("Test Answer", result.Answer);
        Assert.Equal($"/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{questionId}?frm-chk-answer=true", result.ChangeLink);
    }

    [Fact]
    public void CreateAnswerSummary_WithSummaryTitle_UsesSummaryTitle()
    {
        var questionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var question = new FormQuestion
        {
            Id = questionId,
            Title = "Test Question",
            SummaryTitle = "Summary Title",
            Type = FormQuestionType.Text
        };
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = questionId,
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer { TextValue = "Test Answer" }
        };

        var result = _service.CreateAnswerSummary(question, questionAnswer, "Test Answer", organisationId, formId, sectionId);

        Assert.Equal("Summary Title", result.Title);
    }

    [Fact]
    public void CreateAnswerSummary_WithNonUkAddress_AddsUkOrNonUkParameter()
    {
        var questionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var question = new FormQuestion
        {
            Id = questionId,
            Title = "Address Question",
            Type = FormQuestionType.Address
        };
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = questionId,
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer
            {
                AddressValue = new Address
                {
                    AddressLine1 = "123 Test Street",
                    TownOrCity = "Test City",
                    Postcode = "12345",
                    CountryName = "United States",
                    Country = "US"
                }
            }
        };

        var result = _service.CreateAnswerSummary(question, questionAnswer, "Address", organisationId, formId, sectionId);

        Assert.Contains("&UkOrNonUk=non-uk", result.ChangeLink);
    }

    [Fact]
    public void CreateAnswerSummary_WithUkAddress_DoesNotAddUkOrNonUkParameter()
    {
        var questionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var question = new FormQuestion
        {
            Id = questionId,
            Title = "Address Question",
            Type = FormQuestionType.Address
        };
        var questionAnswer = new QuestionAnswer
        {
            QuestionId = questionId,
            AnswerId = Guid.NewGuid(),
            Answer = new FormAnswer
            {
                AddressValue = new Address
                {
                    AddressLine1 = "123 Test Street",
                    TownOrCity = "Test City",
                    Postcode = "SW1A 1AA",
                    CountryName = "United Kingdom",
                    Country = Country.UKCountryCode
                }
            }
        };

        var result = _service.CreateAnswerSummary(question, questionAnswer, "Address", organisationId, formId, sectionId);

        Assert.DoesNotContain("&UkOrNonUk=non-uk", result.ChangeLink);
    }

    [Fact]
    public async Task CreateIndividualAnswerSummaryAsync_WithValidAnswer_ReturnsAnswerSummary()
    {
        var questionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var question = new FormQuestion
        {
            Id = questionId,
            Title = "Test Question",
            Type = FormQuestionType.Text
        };
        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = questionId,
                    AnswerId = Guid.NewGuid(),
                    Answer = new FormAnswer { TextValue = "Test Answer" }
                }
            }
        };

        var result = await _service.CreateIndividualAnswerSummaryAsync(question, answerState, organisationId, formId, sectionId);

        Assert.NotNull(result);
        Assert.Equal("Test Question", result.Title);
        Assert.Equal("Test Answer", result.Answer);
    }

    [Fact]
    public async Task CreateIndividualAnswerSummaryAsync_WithNoMatchingAnswer_ReturnsNull()
    {
        var questionId = Guid.NewGuid();
        var differentQuestionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var question = new FormQuestion
        {
            Id = questionId,
            Title = "Test Question",
            Type = FormQuestionType.Text
        };
        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = differentQuestionId,
                    AnswerId = Guid.NewGuid(),
                    Answer = new FormAnswer { TextValue = "Test Answer" }
                }
            }
        };

        var result = await _service.CreateIndividualAnswerSummaryAsync(question, answerState, organisationId, formId, sectionId);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateIndividualAnswerSummaryAsync_WithEmptyAnswerString_ReturnsNull()
    {
        var questionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var question = new FormQuestion
        {
            Id = questionId,
            Title = "Test Question",
            Type = FormQuestionType.Text
        };
        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = questionId,
                    AnswerId = Guid.NewGuid(),
                    Answer = new FormAnswer { TextValue = "" }
                }
            }
        };

        var result = await _service.CreateIndividualAnswerSummaryAsync(question, answerState, organisationId, formId, sectionId);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateMultiQuestionGroupAsync_WithPageGrouping_CreatesGroupedAnswerSummary()
    {
        var groupingId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var startingQuestionId = Guid.NewGuid();
        var question1Id = Guid.NewGuid();
        var question2Id = Guid.NewGuid();

        var grouping = new FormQuestionGrouping
        {
            Id = groupingId,
            SummaryTitle = "Group Summary",
            Page = true
        };

        var startingQuestion = new FormQuestion
        {
            Id = startingQuestionId,
            Title = "Starting Question",
            SummaryTitle = "Starting Summary",
            Type = FormQuestionType.Text
        };

        var orderedJourney = new List<FormQuestion>
        {
            new FormQuestion
            {
                Id = question1Id,
                Title = "Question 1",
                Type = FormQuestionType.Text,
                Options = new FormQuestionOptions { Grouping = grouping }
            },
            new FormQuestion
            {
                Id = question2Id,
                Title = "Question 2",
                Type = FormQuestionType.Text,
                Options = new FormQuestionOptions { Grouping = grouping }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = question1Id,
                    AnswerId = Guid.NewGuid(),
                    Answer = new FormAnswer { TextValue = "Answer 1" }
                },
                new QuestionAnswer
                {
                    QuestionId = question2Id,
                    AnswerId = Guid.NewGuid(),
                    Answer = new FormAnswer { TextValue = "Answer 2" }
                }
            }
        };

        var getFirstQuestion = (List<FormQuestion> questions) => questions.FirstOrDefault();

        var result = await _service.CreateMultiQuestionGroupAsync(
            startingQuestion, orderedJourney, answerState, organisationId, formId, sectionId, grouping, getFirstQuestion);

        Assert.Equal("Group Summary", result.GroupTitle);
        Assert.Equal($"/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{question1Id}", result.GroupChangeLink);
        Assert.Equal(2, result.Answers.Count);
        Assert.Equal("Question 1", result.Answers[0].Title);
        Assert.Equal("Answer 1", result.Answers[0].Answer);
        Assert.Equal("Question 2", result.Answers[1].Title);
        Assert.Equal("Answer 2", result.Answers[1].Answer);
    }

    [Fact]
    public async Task CreateMultiQuestionGroupAsync_WithNonPageGrouping_SetsIndividualChangeLinks()
    {
        var groupingId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var startingQuestionId = Guid.NewGuid();
        var question1Id = Guid.NewGuid();

        var grouping = new FormQuestionGrouping
        {
            Id = groupingId,
            SummaryTitle = "Group Summary",
            Page = false
        };

        var startingQuestion = new FormQuestion
        {
            Id = startingQuestionId,
            Title = "Starting Question",
            Type = FormQuestionType.Text
        };

        var orderedJourney = new List<FormQuestion>
        {
            new FormQuestion
            {
                Id = question1Id,
                Title = "Question 1",
                Type = FormQuestionType.Text,
                Options = new FormQuestionOptions { Grouping = grouping }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = question1Id,
                    AnswerId = Guid.NewGuid(),
                    Answer = new FormAnswer { TextValue = "Answer 1" }
                }
            }
        };

        var getFirstQuestion = (List<FormQuestion> questions) => questions.FirstOrDefault();

        var result = await _service.CreateMultiQuestionGroupAsync(
            startingQuestion, orderedJourney, answerState, organisationId, formId, sectionId, grouping, getFirstQuestion);

        Assert.Null(result.GroupChangeLink);
        Assert.Equal($"/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{question1Id}?frm-chk-answer=true", result.Answers[0].ChangeLink);
    }

    [Fact]
    public async Task CreateMultiQuestionGroupAsync_WithNoGroupSummaryTitle_UsesStartingQuestionSummaryTitle()
    {
        var groupingId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var startingQuestionId = Guid.NewGuid();

        var grouping = new FormQuestionGrouping
        {
            Id = groupingId,
            SummaryTitle = "",
            Page = true
        };

        var startingQuestion = new FormQuestion
        {
            Id = startingQuestionId,
            Title = "Starting Question",
            SummaryTitle = "Starting Summary Title",
            Type = FormQuestionType.Text
        };

        var orderedJourney = new List<FormQuestion>();
        var answerState = new FormQuestionAnswerState { Answers = new List<QuestionAnswer>() };
        var getFirstQuestion = (List<FormQuestion> questions) => questions.FirstOrDefault();

        var result = await _service.CreateMultiQuestionGroupAsync(
            startingQuestion, orderedJourney, answerState, organisationId, formId, sectionId, grouping, getFirstQuestion);

        Assert.Equal("Starting Summary Title", result.GroupTitle);
    }
}