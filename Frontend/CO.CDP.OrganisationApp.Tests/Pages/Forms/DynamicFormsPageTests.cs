using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;
public class DynamicFormsPageTests
{
    private readonly Mock<IFormsEngine> _formsEngineMock;
    private readonly Mock<ITempDataService> _tempDataServiceMock;
    private readonly DynamicFormsPageModel _pageModel;
    private readonly Guid _organisationId;
    private readonly Guid _formId;
    private readonly Guid _sectionId;

    public DynamicFormsPageTests()
    {
        _formsEngineMock = new Mock<IFormsEngine>();
        _tempDataServiceMock = new Mock<ITempDataService>();
        _pageModel = new DynamicFormsPageModel(_formsEngineMock.Object, _tempDataServiceMock.Object);

        _organisationId = Guid.NewGuid();
        _formId = Guid.NewGuid();
        _sectionId = Guid.NewGuid();
    }

    [Fact]
    public async Task OnGetAsync_ShouldLoadSectionAndSetCurrentQuestion_WhenQuestionIdIsProvided()
    {
        var questionId = Guid.NewGuid();
        var sectionQuestionsResponse = SetupMockLoadFormSectionAsync();
        var currentQuestion = sectionQuestionsResponse.Questions!.First();
        SetupMockGetCurrentQuestion(currentQuestion);

        await _pageModel.OnGetAsync(_organisationId, _formId, _sectionId, questionId);

        _pageModel.SectionWithQuestions.Should().BeEquivalentTo(sectionQuestionsResponse);
        _pageModel.CurrentQuestion.Should().BeEquivalentTo(currentQuestion);
    }

    [Fact]
    public async Task OnGetAsync_ShouldSetFirstQuestionAsCurrent_WhenQuestionIdIsNotProvided()
    {
        var sectionQuestionsResponse = SetupMockLoadFormSectionAsync();
        var firstQuestion = sectionQuestionsResponse.Questions!.First();

        await _pageModel.OnGetAsync(_organisationId, _formId, _sectionId, null);

        _pageModel.SectionWithQuestions.Should().BeEquivalentTo(sectionQuestionsResponse);
        _pageModel.CurrentQuestion.Should().BeEquivalentTo(firstQuestion);
    }

    [Fact]
    public async Task OnPostAsync_ShouldSetCurrentQuestionToNext_WhenActionIsNext()
    {
        var currentQuestionId = Guid.NewGuid();
        var sectionQuestionsResponse = SetupMockLoadFormSectionAsync();
        var nextQuestion = sectionQuestionsResponse.Questions!.Skip(1).First();
        SetupMockGetNextQuestion(nextQuestion);

        await _pageModel.OnPostAsync(_organisationId, _formId, _sectionId, currentQuestionId, "next");

        _pageModel.CurrentQuestion.Should().BeEquivalentTo(nextQuestion);
    }

    [Fact]
    public async Task OnPostAsync_ShouldSetCurrentQuestionToPrevious_WhenActionIsBack()
    {
        var currentQuestionId = Guid.NewGuid();
        var sectionQuestionsResponse = SetupMockLoadFormSectionAsync();
        var previousQuestion = sectionQuestionsResponse.Questions!.First();
        SetupMockGetPreviousQuestion(previousQuestion);

        await _pageModel.OnPostAsync(_organisationId, _formId, _sectionId, currentQuestionId, "back");

        _pageModel.CurrentQuestion.Should().BeEquivalentTo(previousQuestion);
    }

    [Fact]
    public void GetPartialViewName_ShouldReturnCorrectPartialViewName_WhenQuestionTypeIsProvided()
    {
        var partialViewName = _pageModel.GetPartialViewName(FormQuestionType.YesOrNo);

        partialViewName.Should().Be("_FormElementYesNoInput");
    }

    private SectionQuestionsResponse SetupMockLoadFormSectionAsync()
    {
        var sectionQuestionsResponse = CreateSectionQuestionsResponse();
        _formsEngineMock
            .Setup(f => f.LoadFormSectionAsync(_formId, _sectionId))
            .ReturnsAsync(sectionQuestionsResponse);
        return sectionQuestionsResponse;
    }

    private void SetupMockGetCurrentQuestion(FormQuestion currentQuestion)
    {
        _formsEngineMock
            .Setup(f => f.GetCurrentQuestion(_formId, _sectionId, It.IsAny<Guid>()))
            .ReturnsAsync(currentQuestion);
    }

    private void SetupMockGetNextQuestion(FormQuestion nextQuestion)
    {
        _formsEngineMock
            .Setup(f => f.GetNextQuestion(_formId, _sectionId, It.IsAny<Guid>()))
            .ReturnsAsync(nextQuestion);
    }

    private void SetupMockGetPreviousQuestion(FormQuestion previousQuestion)
    {
        _formsEngineMock
            .Setup(f => f.GetPreviousQuestion(_formId, _sectionId, It.IsAny<Guid>()))
            .ReturnsAsync(previousQuestion);
    }

    private SectionQuestionsResponse CreateSectionQuestionsResponse()
    {
        return new SectionQuestionsResponse
        {
            Section = new FormSection { Id = Guid.NewGuid(), Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<FormQuestion>
                {
                    new FormQuestion { Id = Guid.NewGuid(), Title = "Question1", Type = FormQuestionType.YesOrNo, IsRequired = true },
                    new FormQuestion { Id = Guid.NewGuid(), Title = "Question2", Type = FormQuestionType.Text, IsRequired = true }
                }
        };
    }
}
