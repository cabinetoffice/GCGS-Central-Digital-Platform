using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormElementMultiQuestionModelTests
{
    private readonly FormElementMultiQuestionModel _model = new();

    [Fact]
    public void Initialize_ShouldSetPropertiesFromMultiQuestionPageModel()
    {
        var questions = CreateTestQuestions();
        var buttonOptions = new ButtonOptions
        {
            Text = "Test_Submit_Button",
            Style = PrimaryButtonStyle.Start
        };
        var multiQuestionPage = new MultiQuestionPageModel
        {
            Questions = questions,
            Button = buttonOptions
        };
        var existingAnswers = new Dictionary<Guid, FormAnswer>();

        _model.Initialize(multiQuestionPage, existingAnswers);

        _model.Questions.Should().HaveCount(3);
        _model.Questions.Should().BeEquivalentTo(questions);
        _model.Button.Should().BeEquivalentTo(buttonOptions);
    }

    [Fact]
    public void Initialize_ShouldCreateQuestionModelsForAllQuestions()
    {
        var questions = CreateTestQuestions();
        var multiQuestionPage = new MultiQuestionPageModel { Questions = questions };
        var existingAnswers = new Dictionary<Guid, FormAnswer>();

        _model.Initialize(multiQuestionPage, existingAnswers);

        foreach (var question in questions)
        {
            var questionModel = _model.GetQuestionModel(question.Id);
            questionModel.Should().NotBeNull();
            questionModel!.CurrentFormQuestionType.Should().Be(question.Type);
            questionModel.Heading.Should().Be(question.Title);
        }
    }

    [Fact]
    public void Initialize_ShouldSetExistingAnswersOnQuestionModels()
    {
        var questions = CreateTestQuestions();
        var textAnswer = new FormAnswer { TextValue = "Test Answer" };
        var boolAnswer = new FormAnswer { BoolValue = true };

        var existingAnswers = new Dictionary<Guid, FormAnswer>
        {
            { questions[0].Id, textAnswer },
            { questions[1].Id, boolAnswer }
        };

        var multiQuestionPage = new MultiQuestionPageModel { Questions = questions };

        _model.Initialize(multiQuestionPage, existingAnswers);

        var textQuestionModel = _model.GetQuestionModel(questions[0].Id);
        textQuestionModel!.GetAnswer().Should().BeEquivalentTo(textAnswer);

        var yesNoQuestionModel = _model.GetQuestionModel(questions[1].Id);
        yesNoQuestionModel!.GetAnswer().Should().BeEquivalentTo(boolAnswer);
    }

    [Fact]
    public void GetQuestionModel_ShouldReturnNullForNonExistentQuestion()
    {
        var questions = CreateTestQuestions();
        var multiQuestionPage = new MultiQuestionPageModel { Questions = questions };
        _model.Initialize(multiQuestionPage, new Dictionary<Guid, FormAnswer>());
        var nonExistentQuestionId = Guid.NewGuid();

        var result = _model.GetQuestionModel(nonExistentQuestionId);

        result.Should().BeNull();
    }

    [Fact]
    public void GetAllAnswers_ShouldReturnAllNonNullAnswers()
    {
        var questions = CreateTestQuestions();
        var multiQuestionPage = new MultiQuestionPageModel { Questions = questions };

        var existingAnswers = new Dictionary<Guid, FormAnswer>
        {
            { questions[0].Id, new FormAnswer { TextValue = "Answer 1" } },
            { questions[1].Id, new FormAnswer { BoolValue = true } }
        };

        _model.Initialize(multiQuestionPage, existingAnswers);

        var result = _model.GetAllAnswers();

        result.Should().HaveCount(2);
        result.Should().ContainKey(questions[0].Id);
        result.Should().ContainKey(questions[1].Id);
        result.Should().NotContainKey(questions[2].Id);

        result[questions[0].Id].TextValue.Should().Be("Answer 1");
        result[questions[1].Id].BoolValue.Should().BeTrue();
    }

    [Fact]
    public void GetAllAnswers_ShouldReturnEmptyDictionaryWhenNoAnswers()
    {
        var questions = CreateTestQuestions();
        var multiQuestionPage = new MultiQuestionPageModel { Questions = questions };
        _model.Initialize(multiQuestionPage, new Dictionary<Guid, FormAnswer>());

        var result = _model.GetAllAnswers();

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(FormQuestionType.Text, typeof(FormElementTextInputModel))]
    [InlineData(FormQuestionType.YesOrNo, typeof(FormElementYesNoInputModel))]
    [InlineData(FormQuestionType.Date, typeof(FormElementDateInputModel))]
    [InlineData(FormQuestionType.SingleChoice, typeof(FormElementSingleChoiceModel))]
    [InlineData(FormQuestionType.GroupedSingleChoice, typeof(FormElementGroupedSingleChoiceModel))]
    [InlineData(FormQuestionType.MultiLine, typeof(FormElementMultiLineInputModel))]
    [InlineData(FormQuestionType.Url, typeof(FormElementUrlInputModel))]
    [InlineData(FormQuestionType.Address, typeof(FormElementAddressModel))]
    [InlineData(FormQuestionType.CheckBox, typeof(FormElementCheckBoxInputModel))]
    [InlineData(FormQuestionType.FileUpload, typeof(FormElementFileUploadModel))]
    [InlineData(FormQuestionType.NoInput, typeof(FormElementNoInputModel))]
    public void Initialize_ShouldCreateCorrectQuestionModelType(FormQuestionType questionType, Type expectedModelType)
    {
        var question = new FormQuestion
        {
            Id = Guid.NewGuid(),
            Type = questionType,
            Title = $"{questionType} Question",
            Options = new FormQuestionOptions()
        };

        var multiQuestionPage = new MultiQuestionPageModel { Questions = [question] };

        _model.Initialize(multiQuestionPage, new Dictionary<Guid, FormAnswer>());
        var questionModel = _model.GetQuestionModel(question.Id);

        questionModel.Should().NotBeNull();
        questionModel.Should().BeOfType(expectedModelType);
    }

    [Fact]
    public void Initialize_ShouldThrowNotSupportedException_ForUnsupportedQuestionType()
    {
        var question = new FormQuestion
        {
            Id = Guid.NewGuid(),
            Type = (FormQuestionType)999, // Invalid question type
            Title = "Invalid Question",
            Options = new FormQuestionOptions()
        };

        var multiQuestionPage = new MultiQuestionPageModel { Questions = [question] };

        Action act = () => _model.Initialize(multiQuestionPage, new Dictionary<Guid, FormAnswer>());

        act.Should().Throw<NotSupportedException>()
           .WithMessage("Question type * is not supported in multi-question pages");
    }

    [Fact]
    public void Properties_ShouldRetainValues_AfterInitialization()
    {
        var questions = CreateTestQuestions();
        var originalQuestionCount = questions.Count;
        var buttonOptions = new ButtonOptions
        {
            Text = "Original_Button",
            Style = PrimaryButtonStyle.Default
        };
        var multiQuestionPage = new MultiQuestionPageModel
        {
            Questions = questions,
            Button = buttonOptions
        };

        _model.Initialize(multiQuestionPage, new Dictionary<Guid, FormAnswer>());

        _model.Questions.Should().HaveCount(originalQuestionCount);
        _model.Questions.Should().BeEquivalentTo(questions);
        _model.Button.Should().BeEquivalentTo(buttonOptions);
    }

    [Fact]
    public void Initialize_ShouldHandleEmptyQuestionsList()
    {
        var multiQuestionPage = new MultiQuestionPageModel
        {
            Questions = []
        };

        _model.Initialize(multiQuestionPage, new Dictionary<Guid, FormAnswer>());

        _model.Questions.Should().BeEmpty();
        _model.Button.Should().BeNull();
        _model.GetAllAnswers().Should().BeEmpty();
    }

    [Fact]
    public void Initialize_ShouldHandleNullButtonOptions()
    {
        var questions = CreateTestQuestions();
        var multiQuestionPage = new MultiQuestionPageModel
        {
            Questions = questions,
            Button = null
        };

        _model.Initialize(multiQuestionPage, new Dictionary<Guid, FormAnswer>());

        _model.Questions.Should().BeEquivalentTo(questions);
        _model.Button.Should().BeNull();
    }

    private static List<FormQuestion> CreateTestQuestions()
    {
        return
        [
            new FormQuestion
            {
                Id = Guid.NewGuid(),
                Type = FormQuestionType.Text,
                Title = "Text Question",
                Description = "Enter some text",
                IsRequired = true,
                Options = new FormQuestionOptions()
            },
            new FormQuestion
            {
                Id = Guid.NewGuid(),
                Type = FormQuestionType.YesOrNo,
                Title = "Yes/No Question",
                Description = "Choose yes or no",
                IsRequired = true,
                Options = new FormQuestionOptions()
            },
            new FormQuestion
            {
                Id = Guid.NewGuid(),
                Type = FormQuestionType.Date,
                Title = "Date Question",
                Description = "Enter a date",
                IsRequired = false,
                Options = new FormQuestionOptions()
            }
        ];
    }
}