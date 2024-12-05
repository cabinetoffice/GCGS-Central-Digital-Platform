using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Moq;

namespace CO.CDP.OrganisationApp.Tests;
public class FlashMessageServiceTests
{
    private readonly Mock<ITempDataService> _tempDataServiceMock;
    private readonly FlashMessageService _flashMessageService;

    public FlashMessageServiceTests()
    {
        _tempDataServiceMock = new Mock<ITempDataService>();
        _flashMessageService = new FlashMessageService(_tempDataServiceMock.Object);
    }

    [Fact]
    public void SetSuccessMessage_ShouldCallTempDataServiceWithSuccessMessageType()
    {
        var heading = "Test success Heading";
        var description = "Test success Description";

        _flashMessageService.SetSuccessMessage(heading, description);

        _tempDataServiceMock.Verify(s =>
            s.Put(FlashMessageTypes.Success,
                  It.Is<FlashMessage>(m => m.Heading == heading && m.Description == description)),
            Times.Once);
    }

    [Fact]
    public void SetSuccessMessage_ShouldCallTempDataServiceWithTitle()
    {
        var heading = "Test success Heading";
        var description = "Test success Description";
        var title = "Test success Title";

        _flashMessageService.SetSuccessMessage(heading, description, title);

        _tempDataServiceMock.Verify(s =>
            s.Put(FlashMessageTypes.Success,
                  It.Is<FlashMessage>(m => m.Heading == heading && m.Description == description && m.Title == title)),
            Times.Once);
    }

    [Fact]
    public void SetImportantMessage_ShouldCallTempDataServiceWithImportantMessageType()
    {
        var heading = "Test important Heading";
        var description = "Test important Description";

        _flashMessageService.SetImportantMessage(heading, description);

        _tempDataServiceMock.Verify(s =>
            s.Put(FlashMessageTypes.Important,
                  It.Is<FlashMessage>(m => m.Heading == heading && m.Description == description)),
            Times.Once);
    }

    [Fact]
    public void GetFlashMessage_ShouldReturnMessageFromTempDataService()
    {
        var messageType = FlashMessageTypes.Success;
        var expectedMessage = new FlashMessage("Test Heading", "Test Description");
        _tempDataServiceMock.Setup(s => s.Get<FlashMessage>(messageType))
            .Returns(expectedMessage);

        var result = _flashMessageService.GetFlashMessage(messageType);

        Assert.Equal(expectedMessage.Heading, result!.Heading);
        Assert.Equal(expectedMessage.Description, result.Description);
        _tempDataServiceMock.Verify(s => s.Get<FlashMessage>(messageType), Times.Once);
    }

    [Fact]
    public void PeekFlashMessage_ShouldReturnMessageFromTempDataServiceWithoutRemovingIt()
    {
        var messageType = FlashMessageTypes.Important;
        var expectedMessage = new FlashMessage("Test Heading", "Test Description");
        _tempDataServiceMock.Setup(s => s.Peek<FlashMessage>(messageType))
            .Returns(expectedMessage);

        var result = _flashMessageService.PeekFlashMessage(messageType);

        Assert.Equal(expectedMessage.Heading, result!.Heading);
        Assert.Equal(expectedMessage.Description, result.Description);
        _tempDataServiceMock.Verify(s => s.Peek<FlashMessage>(messageType), Times.Once);
    }

    [Fact]
    public void GetFlashMessage_ShouldReturnNull_WhenMessageDoesNotExist()
    {
        var messageType = FlashMessageTypes.Success;
        _tempDataServiceMock.Setup(s => s.Get<FlashMessage>(messageType))
            .Returns((FlashMessage?)null);

        var result = _flashMessageService.GetFlashMessage(messageType);

        Assert.Null(result);
        _tempDataServiceMock.Verify(s => s.Get<FlashMessage>(messageType), Times.Once);
    }

    [Fact]
    public void PeekFlashMessage_ShouldReturnNull_WhenMessageDoesNotExist()
    {
        var messageType = FlashMessageTypes.Important;
        _tempDataServiceMock.Setup(s => s.Peek<FlashMessage>(messageType))
            .Returns((FlashMessage?)null);

        var result = _flashMessageService.PeekFlashMessage(messageType);

        Assert.Null(result);
        _tempDataServiceMock.Verify(s => s.Peek<FlashMessage>(messageType), Times.Once);
    }

    [Fact]
    public void SetSuccessMessage_ShouldFormatHeadingAndDescriptionWithParameters()
    {
        var formatString = "Visit <a href=\"/organisation/{organisationId}\">{organisationName}</a>";
        var urlParameters = new Dictionary<string, string> { { "organisationId", "12345" } };
        var htmlParameters = new Dictionary<string, string> { { "organisationName", "Test Organisation" } };

        var expectedOutput = "Visit <a href=\"/organisation/12345\">Test Organisation</a>";

        _flashMessageService.SetSuccessMessage(formatString, formatString, null, urlParameters, htmlParameters);

        _tempDataServiceMock.Verify(s =>
            s.Put(FlashMessageTypes.Success,
                It.Is<FlashMessage>(m => m.Heading == expectedOutput && m.Description == expectedOutput)),
            Times.Once);
    }

    [Fact]
    public void SetImportantMessage_ShouldHandleNullDescriptionAndFormatHeading()
    {
        var heading = "Access <a href=\"/organisation/{organisationId}\">your account</a>.";
        var urlParameters = new Dictionary<string, string> { { "organisationId", "67890" } };

        var expectedHeading = "Access <a href=\"/organisation/67890\">your account</a>.";

        _flashMessageService.SetImportantMessage(heading, null, null,urlParameters, null);

        _tempDataServiceMock.Verify(s =>
            s.Put(FlashMessageTypes.Important,
                It.Is<FlashMessage>(m => m.Heading == expectedHeading && m.Description == null)),
            Times.Once);
    }

    [Fact]
    public void SetImportantMessage_ShouldIgnoreUnusedPlaceholders()
    {
        var heading = "Hello {userName}";
        var urlParameters = new Dictionary<string, string> { { "irrelevantParam", "12345" } };
        var htmlParameters = new Dictionary<string, string> { { "userName", "Alice" } };

        var expectedHeading = "Hello Alice";

        _flashMessageService.SetImportantMessage(heading, null, null, urlParameters, htmlParameters);

        _tempDataServiceMock.Verify(s =>
            s.Put(FlashMessageTypes.Important,
                It.Is<FlashMessage>(m => m.Heading == expectedHeading && m.Description == null)),
            Times.Once);
    }

    [Fact]
    public void SetSuccessMessage_ShouldFormatNotBeVulnerabletoXSS()
    {
        var formatString = "Visit <a href=\"/organisation/{organisationId}\">{organisationName}</a>";
        var urlParameters = new Dictionary<string, string> { { "organisationId", "<script>alert(\"hello\")</script>" } };
        var htmlParameters = new Dictionary<string, string> { { "organisationName", "<script>alert(\"hello\")</script>" } };

        var expectedOutput = "Visit <a href=\"/organisation/%3Cscript%3Ealert(%22hello%22)%3C%2Fscript%3E\">&lt;script&gt;alert(&quot;hello&quot;)&lt;/script&gt;</a>";

        _flashMessageService.SetSuccessMessage(formatString, formatString, formatString, urlParameters, htmlParameters);

        _tempDataServiceMock.Verify(s =>
            s.Put(FlashMessageTypes.Success,
                It.Is<FlashMessage>(m => m.Heading == expectedOutput && m.Description == expectedOutput && m.Title == expectedOutput)),
            Times.Once);
    }
}
