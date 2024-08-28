using CO.CDP.AwsServices;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class DynamicFormsPageModelTest
{
    private readonly Mock<IFormsEngine> _formsEngineMock;
    private readonly Mock<ITempDataService> _tempDataServiceMock;
    private readonly Mock<IFileHostManager> _fileHostManagerMock;
    private readonly DynamicFormsPageModel _pageModel;
    private readonly Guid TextQuestionId = Guid.NewGuid();

    public DynamicFormsPageModelTest()
    {
        _formsEngineMock = new Mock<IFormsEngine>();

        var form = new SectionQuestionsResponse
        {
            Questions = [new FormQuestion { Id = TextQuestionId, Type = FormQuestionType.Text, SummaryTitle = "Sample Question" }]
        };

        _formsEngineMock.Setup(f => f.GetFormSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(form);
        _tempDataServiceMock = new Mock<ITempDataService>();
        _fileHostManagerMock = new Mock<IFileHostManager>();
        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(new FormQuestionAnswerState());
        _pageModel = new DynamicFormsPageModel(_formsEngineMock.Object, _tempDataServiceMock.Object, _fileHostManagerMock.Object);
    }

    [Fact]
    public async Task OnGetAsync_RedirectsToPageNotFound_WhenCurrentQuestionIsNull()
    {
        _formsEngineMock.Setup(f => f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync((FormQuestion?)null);

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGetAsync_ReturnsPage_WhenCurrentQuestionIsNotNull()
    {
        var formQuestion = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.Text };
        _formsEngineMock.Setup(f => f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync(formQuestion);

        var result = await _pageModel.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _pageModel.PartialViewName.Should().Be("_FormElementTextInput");
        _pageModel.PartialViewModel.Should().NotBeNull();
    }

    [Fact]
    public async Task OnPostAsync_RedirectsToPageNotFound_WhenCurrentQuestionIsNull()
    {
        _formsEngineMock.Setup(f => f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync((FormQuestion?)null);

        var result = await _pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task EncType_ReturnsMultipartFormData_WhenCurrentFormQuestionTypeIsFileUpload()
    {
        var formQuestion = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.FileUpload };
        _formsEngineMock.Setup(f => f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync(formQuestion);

        var result = await _pageModel.OnGetAsync();

        _pageModel.EncType.Should().Be("multipart/form-data");
    }

    [Fact]
    public async Task EncType_ReturnsUrlEncoded_WhenCurrentFormQuestionTypeIsNotFileUpload()
    {
        var formQuestion = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.Text };
        _formsEngineMock.Setup(f => f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync(formQuestion);

        var result = await _pageModel.OnGetAsync();

        _pageModel.EncType.Should().Be("application/x-www-form-urlencoded");
    }

    [Fact]
    public async Task GetAnswers_ShouldReturnCheckYouAnswersSummaries()
    {
        var answerSet = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
        {
            new QuestionAnswer
            {
                QuestionId = TextQuestionId,
                Answer = new FormAnswer { TextValue = "Sample Answer" }
            }
        }
        };

        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(answerSet);

        var answers = await _pageModel.GetAnswers();

        answers.Should().HaveCount(1);
        answers.First().Answer.Should().Be("Sample Answer");
        answers.First().Title.Should().Be("Sample Question");
    }

    [Fact]
    public async Task OnPostAsync_ShouldCallSaveUpdateAnswersAndRedirectToFormsAddAnotherAnswerSet_WhenCurrentQuestionIsCheckYourAnswerQuestion()
    {
        var currentQuestionId = Guid.NewGuid();
        var checkYourAnswerQuestionId = currentQuestionId;
        var formQuestion = new FormQuestion { Id = currentQuestionId, Type = FormQuestionType.CheckYourAnswers };

        _formsEngineMock.Setup(f => f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync(formQuestion);

        _formsEngineMock.Setup(f => f.GetFormSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new SectionQuestionsResponse
            {
                Questions = new List<FormQuestion>
                {
                new FormQuestion { Id = checkYourAnswerQuestionId, Type = FormQuestionType.CheckYourAnswers }
                }
            });

        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(new FormQuestionAnswerState());
        var result = await _pageModel.OnPostAsync();
        _formsEngineMock.Verify(f => f.SaveUpdateAnswers(_pageModel.FormId, _pageModel.SectionId, _pageModel.OrganisationId, It.IsAny<FormQuestionAnswerState>()), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
              .Which.PageName.Should().Be("FormsAddAnotherAnswerSet");
    }

    [Fact]
    public async Task OnPostAsync_ShouldRedirectToShareCodeConfirmation_WhenSectionTitleIsDeclarationInformation()
    {
        var checkYourAnswerQuestionId = Guid.NewGuid();
        _pageModel.FormSectionType = FormSectionType.Declaration;
        _pageModel.CurrentQuestionId = checkYourAnswerQuestionId;

        var formResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Declaration, Title = "Test Section" },
            Questions = [new FormQuestion { Id = checkYourAnswerQuestionId, Type = FormQuestionType.CheckYourAnswers }]
        };

        _formsEngineMock.Setup(f => f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new FormQuestion { Id = checkYourAnswerQuestionId, Type = FormQuestionType.CheckYourAnswers });

        _formsEngineMock.Setup(f => f.GetFormSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(formResponse);

        var result = await _pageModel.OnPostAsync();

        _formsEngineMock.Verify(f => f.SaveUpdateAnswers(_pageModel.FormId, _pageModel.SectionId, _pageModel.OrganisationId, It.IsAny<FormQuestionAnswerState>()), Times.Once);
        result.Should().BeOfType<RedirectToPageResult>()
              .Which.PageName.Should().Be("/ShareInformation/ShareCodeConfirmation");
    }

    [Fact]
    public async Task OnPostAsync_ShouldRedirectToShareCodeConfirmation_WithGeneratedShareCode_WhenSectionIsDeclaration()
    {
        var shareCode = "HDJ2123F";
        _pageModel.FormSectionType = FormSectionType.Declaration;
        _pageModel.CurrentQuestionId = Guid.NewGuid();

        var formResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Type = FormSectionType.Declaration, Title = "Test Section" },
            Questions = new List<FormQuestion>
            {
                new FormQuestion { Id = _pageModel.CurrentQuestionId.Value, Type = FormQuestionType.CheckYourAnswers }
            }
        };

        _formsEngineMock.Setup(f => f.GetCurrentQuestion(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new FormQuestion { Id = _pageModel.CurrentQuestionId.Value, Type = FormQuestionType.CheckYourAnswers });

        _formsEngineMock.Setup(f => f.GetFormSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(formResponse);

        _formsEngineMock.Setup(f => f.CreateShareCodeAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(shareCode);

        _tempDataServiceMock.Setup(t => t.PeekOrDefault<FormQuestionAnswerState>(It.IsAny<string>()))
            .Returns(new FormQuestionAnswerState());

        var result = await _pageModel.OnPostAsync();

        _formsEngineMock.Verify(f => f.CreateShareCodeAsync(_pageModel.FormId, _pageModel.OrganisationId), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
              .Which.PageName.Should().Be("/ShareInformation/ShareCodeConfirmation");
        (result as RedirectToPageResult)!.RouteValues?["shareCode"].Should().Be(shareCode);
    }
}