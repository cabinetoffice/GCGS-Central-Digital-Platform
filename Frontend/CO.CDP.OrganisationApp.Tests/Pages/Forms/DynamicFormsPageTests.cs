using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;
public class DynamicFormsPageTests
{
    private readonly Mock<IFormsEngine> _formsEngineMock;
    private readonly Mock<ITempDataService> _tempDataServiceMock;
    private readonly Mock<HttpRequest> _requestMock;
    private readonly Mock<HttpContext> _httpContextMock;
    private readonly DynamicFormsPageModel _pageModel;
    private readonly Guid _organisationId;
    private readonly Guid _formId;
    private readonly Guid _sectionId;

    public DynamicFormsPageTests()
    {
        _formsEngineMock = new Mock<IFormsEngine>();
        _tempDataServiceMock = new Mock<ITempDataService>();
        _requestMock = new Mock<HttpRequest>();
        _httpContextMock = new Mock<HttpContext>();
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

        VerifySectionAndCurrentQuestion(sectionQuestionsResponse, currentQuestion);
    }

    [Fact]
    public async Task OnGetAsync_ShouldSetFirstQuestionAsCurrent_WhenQuestionIdIsNotProvided()
    {
        var sectionQuestionsResponse = SetupMockLoadFormSectionAsync();
        var firstQuestion = sectionQuestionsResponse.Questions!.First();

        await _pageModel.OnGetAsync(_organisationId, _formId, _sectionId, null);

        VerifySectionAndCurrentQuestion(sectionQuestionsResponse, firstQuestion);
    }

    [Fact]
    public async Task OnPostAsync_ShouldSetCurrentQuestionToNext_WhenActionIsNext()
    {
        var sectionQuestionsResponse = SetupMockLoadFormSectionAsync();
        var currentQuestionId = sectionQuestionsResponse.Questions!.First().Id;
        var nextQuestion = sectionQuestionsResponse.Questions!.Skip(1).First();
        SetupMockGetNextQuestion(nextQuestion);

        _pageModel.CurrentQuestion = sectionQuestionsResponse.Questions!.First();
        _pageModel.Answer = "some answer";

        await _pageModel.OnPostAsync(_organisationId, _formId, _sectionId, currentQuestionId, "next");

        _pageModel.CurrentQuestion.Should().BeEquivalentTo(nextQuestion);
    }

    [Fact]
    public async Task OnPostAsync_ShouldSetCurrentQuestionToPrevious_WhenActionIsBack()
    {
        var sectionQuestionsResponse = SetupMockLoadFormSectionAsync();
        var currentQuestionId = sectionQuestionsResponse.Questions!.Skip(1).First().Id;
        var previousQuestion = sectionQuestionsResponse.Questions!.First();
        SetupMockGetPreviousQuestion(previousQuestion);

        _pageModel.CurrentQuestion = sectionQuestionsResponse.Questions!.Skip(1).First();
        _pageModel.Answer = "some answer";

        await _pageModel.OnPostAsync(_organisationId, _formId, _sectionId, currentQuestionId, "back");

        _pageModel.CurrentQuestion.Should().BeEquivalentTo(previousQuestion);
    }

    [Fact]
    public async Task OnPostAsync_ShouldReturnPageWithErrors_WhenYesNoAnswerIsNotProvided()
    {
        var sectionQuestionsResponse = SetupMockLoadFormSectionAsync();
        var currentQuestion = sectionQuestionsResponse.Questions!.First(q => q.Type == FormQuestionType.YesOrNo);
        var currentQuestionId = currentQuestion.Id;

        _pageModel.SectionWithQuestions = sectionQuestionsResponse;
        _pageModel.CurrentQuestion = currentQuestion;

        SetupMockGetCurrentQuestion(currentQuestion);

        _pageModel.Answer = null;

        var result = await _pageModel.OnPostAsync(_organisationId, _formId, _sectionId, currentQuestionId, "next");

        result.Should().BeOfType<PageResult>();
        _pageModel.ModelState.IsValid.Should().BeFalse();
        _pageModel.ModelState["YesNoAnswer"]?.Errors.Should().ContainSingle(e => e.ErrorMessage == "Please select an option.");
    }

    [Fact]
    public void OnPostAsync_ShouldReturnPageWithErrors_WhenFileUploadIsNotProvided()
    {
        var sectionQuestionsResponse = SetupMockLoadFormSectionAsync();
        var currentQuestion = sectionQuestionsResponse.Questions!.First(q => q.Type == FormQuestionType.FileUpload);
        var currentQuestionId = currentQuestion.Id;

        var formFileCollection = new FormFileCollection();
        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), formFileCollection);

        _requestMock.Setup(x => x.Form).Returns(formCollection);

        _pageModel.Request = _requestMock.Object;

        var result = _pageModel.ValidateFileUpload();

        Assert.False(result);
        Assert.True(_pageModel.ModelState.ContainsKey("Answer"));
        Assert.Equal("No file selected.", _pageModel.ModelState["Answer"]?.Errors?[0].ErrorMessage);
    }

    [Fact]
    public void OnPostAsync_ShouldReturnPageWithErrors_WhenFileUploadSizeInvalid()
    {
        var sectionQuestionsResponse = SetupMockLoadFormSectionAsync();
        var currentQuestion = sectionQuestionsResponse.Questions!.First(q => q.Type == FormQuestionType.FileUpload);
        var currentQuestionId = currentQuestion.Id;

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(11 * 1024 * 1024); // 11 MB file
        fileMock.Setup(f => f.Name).Returns("UploadedFile");

        var formFileCollection = new FormFileCollection { fileMock.Object };
        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), formFileCollection);

        _requestMock.Setup(x => x.Form).Returns(formCollection);

        _pageModel.Request = _requestMock.Object;

        var result = _pageModel.ValidateFileUpload();

        Assert.False(result);
        Assert.True(_pageModel.ModelState.ContainsKey("Answer"));
        Assert.Equal("The file size must not exceed 10MB.", _pageModel.ModelState["Answer"]?.Errors?[0].ErrorMessage);
    }

    [Fact]
    public void OnPostAsync_ShouldReturnPageWithErrors_WhenFileUploadTypeInvalid()
    {
        var sectionQuestionsResponse = SetupMockLoadFormSectionAsync();
        var currentQuestion = sectionQuestionsResponse.Questions!.First(q => q.Type == FormQuestionType.FileUpload);
        var currentQuestionId = currentQuestion.Id;

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(5 * 1024 * 1024); // 11 MB file
        fileMock.Setup(f => f.Name).Returns("UploadedFile");
        fileMock.Setup(f => f.FileName).Returns("badfile.exe");
        var formFileCollection = new FormFileCollection { fileMock.Object };
        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), formFileCollection);

        _requestMock.Setup(x => x.Form).Returns(formCollection);
        _pageModel.Request = _requestMock.Object;

        var result = _pageModel.ValidateFileUpload();

        Assert.False(result);
        Assert.True(_pageModel.ModelState.ContainsKey("Answer"));
        Assert.Equal("Please upload a file which has one of the following extensions: .pdf, .docx, .csv, .jpg, .bmp, .png, .tif", _pageModel.ModelState["Answer"]?.Errors?[0].ErrorMessage);
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

    private void VerifySectionAndCurrentQuestion(SectionQuestionsResponse sectionQuestionsResponse, FormQuestion currentQuestion)
    {
        _pageModel.SectionWithQuestions.Should().BeEquivalentTo(sectionQuestionsResponse);
        _pageModel.CurrentQuestion.Should().BeEquivalentTo(currentQuestion);
    }

    private SectionQuestionsResponse CreateSectionQuestionsResponse()
    {
        return new SectionQuestionsResponse
        {
            Section = new FormSection { Id = Guid.NewGuid(), Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<FormQuestion>
                    {
                        new FormQuestion { Id = Guid.NewGuid(), Title = "Question1", Type = FormQuestionType.YesOrNo, IsRequired = true },
                        new FormQuestion { Id = Guid.NewGuid(), Title = "Question2", Type = FormQuestionType.Text, IsRequired = true },
                        new FormQuestion { Id = Guid.NewGuid(), Title = "Question3", Type = FormQuestionType.FileUpload, IsRequired = true }
                    }
        };
    }
}