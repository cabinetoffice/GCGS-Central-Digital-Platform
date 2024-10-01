using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormElementFileUploadModelTest
{
    private readonly FormElementFileUploadModel _model;

    public FormElementFileUploadModelTest()
    {
        _model = new FormElementFileUploadModel();
    }

    [Fact]
    public void GetAnswer_ReturnsNull_WhenUploadedFileNameIsEmpty()
    {
        _model.UploadedFileName = null;

        var answer = _model.GetAnswer();

        answer.Should().BeNull();
    }

    [Fact]
    public void GetAnswer_ReturnsAnswer_WhenUploadedFileNameIsNotEmpty()
    {
        _model.UploadedFileName = "testfile.txt";

        var answer = _model.GetAnswer();

        answer.Should().NotBeNull();
        answer!.TextValue.Should().Be("testfile.txt");
    }

    [Fact]
    public void SetAnswer_SetsUploadedFileName_WhenAnswerIsValid()
    {
        var answer = new FormAnswer { TextValue = "testfile.txt" };

        _model.SetAnswer(answer);

        _model.UploadedFileName.Should().Be("testfile.txt");
    }

    [Fact]
    public void SetAnswer_DoesNotSetUploadedFileName_WhenAnswerIsNull()
    {
        _model.SetAnswer(null);

        _model.UploadedFileName.Should().BeNull();
    }

    [Fact]
    public void GetUploadedFileInfo_ReturnsNull_WhenUploadedFileIsNull()
    {
        var fileInfo = _model.GetUploadedFileInfo();

        fileInfo.Should().BeNull();
    }

    [Fact]
    public void GetUploadedFileInfo_ReturnsFileInfo_WhenUploadedFileIsNotNull()
    {
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.FileName).Returns("tes<t file:name*.txt");
        formFileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        _model.UploadedFile = formFileMock.Object;

        var fileInfo = _model.GetUploadedFileInfo();

        fileInfo.Should().NotBeNull();
        fileInfo!.Value.formFile.Should().Be(_model.UploadedFile);
        fileInfo.Value.contentType.Should().Be("text/plain");
        fileInfo.Value.filename.Should().NotContainAny(" ", "<", ":", "*");
        fileInfo.Value.filename.Should().StartWith("tes_t_file_name_");
        fileInfo.Value.filename.Should().EndWith(".txt");
    }

    [Fact]
    public void Validate_ReturnsError_WhenFileIsRequiredButNotUploaded()
    {
        _model.IsRequired = true;
        _model.CurrentFormQuestionType = FormQuestionType.FileUpload;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().ContainSingle();
        validationResults.First().ErrorMessage.Should().Be("No file selected.");
    }

    [Fact]
    public void Validate_ReturnsError_WhenUploadedFileExceedsMaxSize()
    {
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(26 * 1024 * 1024); // 26 MB
        formFileMock.Setup(f => f.FileName).Returns("testfile.txt");

        _model.IsRequired = true;
        _model.CurrentFormQuestionType = FormQuestionType.FileUpload;
        _model.UploadedFile = formFileMock.Object;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().ContainSingle();
        validationResults.First().ErrorMessage.Should().Be("The file size must not exceed 25MB.");
    }

    [Fact]
    public void Validate_ReturnsError_WhenUploadedFileHasInvalidExtension()
    {
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(1 * 1024 * 1024); // 1 MB
        formFileMock.Setup(f => f.FileName).Returns("testfile.exe");

        _model.IsRequired = true;
        _model.CurrentFormQuestionType = FormQuestionType.FileUpload;
        _model.UploadedFile = formFileMock.Object;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().ContainSingle();
        validationResults.First().ErrorMessage.Should().Be("Please upload a file which has one of the following extensions: .jpg, .jpeg, .png, .pdf, .txt, .xls, .xlsx, .csv, .docx, .doc");
    }

    [Fact]
    public void Validate_ReturnsError_WhenUploadedFileHasInvalidExtensionEvenAFileAlreadyUploaded()
    {
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(1 * 1024 * 1024); // 1 MB
        formFileMock.Setup(f => f.FileName).Returns("testfile.exe");

        _model.IsRequired = true;
        _model.CurrentFormQuestionType = FormQuestionType.FileUpload;
        _model.UploadedFile = formFileMock.Object;
        _model.UploadedFileName = "test.jpg";

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().ContainSingle();
        validationResults.First().ErrorMessage.Should().Be("Please upload a file which has one of the following extensions: .jpg, .jpeg, .png, .pdf, .txt, .xls, .xlsx, .csv, .docx, .doc");
    }

    [Fact]
    public void Validate_ReturnsNoErrors_WhenUploadedFileIsValid()
    {
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(1 * 1024 * 1024); // 1 MB
        formFileMock.Setup(f => f.FileName).Returns("testfile.txt");

        _model.IsRequired = true;
        _model.CurrentFormQuestionType = FormQuestionType.FileUpload;
        _model.UploadedFile = formFileMock.Object;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        validationResults.Should().BeEmpty();
    }
}