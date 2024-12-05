namespace CO.CDP.OrganisationApp;

using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;

public class FlashMessageService(ITempDataService tempDataService) : IFlashMessageService
{
    public void SetSuccessMessage(string heading, string? description = null)
    {
        SetFlashMessage(FlashMessageTypes.Success, new FlashMessage(heading, description));
    }

    public void SetImportantMessage(string heading, string? description = null)
    {
        SetFlashMessage(FlashMessageTypes.Important, new FlashMessage(heading, description));
    }

    public FlashMessage? GetFlashMessage(string messageType)
    {
        return tempDataService.Get<FlashMessage>(messageType);
    }

    public FlashMessage? PeekFlashMessage(string messageType)
    {
        return tempDataService.Peek<FlashMessage>(messageType);
    }

    private void SetFlashMessage(string messageType, FlashMessage message)
    {
        tempDataService.Put(messageType, message);
    }
}


public interface IFlashMessageService
{
    void SetSuccessMessage(string heading, string? description = null);
    void SetImportantMessage(string heading, string? description = null);
    FlashMessage? GetFlashMessage(string messageType);
    FlashMessage? PeekFlashMessage(string messageType);
}
