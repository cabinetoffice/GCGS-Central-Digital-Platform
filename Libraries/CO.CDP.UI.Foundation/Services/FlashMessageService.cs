using System.Net;
using CO.CDP.UI.Foundation.Models;

namespace CO.CDP.UI.Foundation.Services;

public class FlashMessageService(ITempDataService tempDataService) : IFlashMessageService
{
    public FlashMessage? GetFlashMessage(FlashMessageType messageType)
    {
        return tempDataService.Get<FlashMessage>(GetMessageKey(messageType));
    }

    public FlashMessage? PeekFlashMessage(FlashMessageType messageType)
    {
        return tempDataService.Peek<FlashMessage>(GetMessageKey(messageType));
    }

    public void SetFlashMessage(FlashMessageType messageType, string heading, string? description = null, string? title = null, Dictionary<string, string>? urlParameters = null, Dictionary<string, string>? htmlParameters = null)
    {
        var formattedHeading = FormatMessage(heading, urlParameters, htmlParameters);
        var formattedDescription = description != null ? FormatMessage(description, urlParameters, htmlParameters) : null;
        var formattedTitle = title != null ? FormatMessage(title, urlParameters, htmlParameters) : null;

        tempDataService.Put(GetMessageKey(messageType), new FlashMessage(formattedHeading, formattedDescription, formattedTitle));
    }

    private static string GetMessageKey(FlashMessageType messageType)
    {
        return "FlashMessage-" + messageType.ToString();
    }

    private static string FormatMessage(string format, Dictionary<string, string>? urlParameters, Dictionary<string, string>? htmlParameters)
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

public enum FlashMessageType
{
    Success,
    Important,
    Failure
}