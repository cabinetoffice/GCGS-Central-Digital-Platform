using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using System.Net;

namespace CO.CDP.OrganisationApp;

public class FlashMessageService(ITempDataService tempDataService) : IFlashMessageService
{
    public void SetSuccessMessage(string heading, string? description = null, string? title = null, Dictionary<string, string>? urlParameters = null, Dictionary<string, string>? htmlParameters = null)
    {
        var formattedHeading = FormatMessage(heading, urlParameters, htmlParameters);
        var formattedDescription = description!= null ? FormatMessage(description, urlParameters, htmlParameters) : null;
        var formattedTitle = title != null ? FormatMessage(title, urlParameters, htmlParameters) : null;
        SetFlashMessage(FlashMessageTypes.Success, new FlashMessage(formattedHeading, formattedDescription, formattedTitle));
    }

    public void SetImportantMessage(string heading, string? description = null, string? title = null, Dictionary<string, string>? urlParameters = null, Dictionary<string, string>? htmlParameters = null)
    {
        var formattedHeading = FormatMessage(heading, urlParameters, htmlParameters);
        var formattedDescription = description!= null ? FormatMessage(description, urlParameters, htmlParameters) : null;
        var formattedTitle = title != null ? FormatMessage(title, urlParameters, htmlParameters) : null;
        SetFlashMessage(FlashMessageTypes.Important, new FlashMessage(formattedHeading, formattedDescription, formattedTitle));
    }

    public void SetFailureMessage(string heading, string? description = null, string? title = null, Dictionary<string, string>? urlParameters = null, Dictionary<string, string>? htmlParameters = null)
    {
        var formattedHeading = FormatMessage(heading, urlParameters, htmlParameters);
        var formattedDescription = description != null ? FormatMessage(description, urlParameters, htmlParameters) : null;
        var formattedTitle = title != null ? FormatMessage(title, urlParameters, htmlParameters) : null;
        SetFlashMessage(FlashMessageTypes.Failure, new FlashMessage(formattedHeading, formattedDescription, formattedTitle));
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

    private string FormatMessage(string format, Dictionary<string, string>? urlParameters, Dictionary<string, string>? htmlParameters)
    {
        var encodedParameters = new Dictionary<string, string>();

        if (urlParameters != null)
        {
            foreach (var kvp in urlParameters)
            {
                encodedParameters[kvp.Key] = WebUtility.UrlEncode(kvp.Value);
            }
        }

        if (htmlParameters != null)
        {
            foreach (var kvp in htmlParameters)
            {
                encodedParameters[kvp.Key] = WebUtility.HtmlEncode(kvp.Value);
            }
        }

        return encodedParameters.Aggregate(format, (current, kvp) =>
            current.Replace($"{{{kvp.Key}}}", kvp.Value));
    }
}


public interface IFlashMessageService
{
    void SetSuccessMessage(
        string heading,
        string? description = null,
        string? title = null,
        Dictionary<string, string>? urlParameters = null,
        Dictionary<string, string>? htmlParameters = null);

    void SetImportantMessage(
        string heading,
        string? description = null,
        string? title = null,
        Dictionary<string, string>? urlParameters = null,
        Dictionary<string, string>? htmlParameters = null);

    FlashMessage? GetFlashMessage(string messageType);

    FlashMessage? PeekFlashMessage(string messageType);
    void SetFailureMessage(string heading, string? description = null, string? title = null, Dictionary<string, string>? urlParameters = null, Dictionary<string, string>? htmlParameters = null);
}
