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

    public DynamicFormsPageModelTest()
    {
        _formsEngineMock = new Mock<IFormsEngine>();
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
}
