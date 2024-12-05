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
}
