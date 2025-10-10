using CO.CDP.UI.Foundation.Models;

namespace CO.CDP.UI.Foundation.Services;

public interface IFlashMessageService
{
    void SetFlashMessage(
        FlashMessageType messageType,
        string heading,
        string? description = null,
        string? title = null,
        Dictionary<string, string>? urlParameters = null,
        Dictionary<string, string>? htmlParameters = null);

    FlashMessage? GetFlashMessage(FlashMessageType messageType);

    FlashMessage? PeekFlashMessage(FlashMessageType messageType);
}