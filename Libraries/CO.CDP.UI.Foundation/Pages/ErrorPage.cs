using CO.CDP.UI.Foundation.Services;

namespace CO.CDP.UI.Foundation.Pages;

public class ErrorPage
{
    public string? TraceId { get; }
    private string? FeedbackUrl { get; }

    public ErrorPage(string? traceId, ISirsiUrlService sirsiUrlService)
    {
        TraceId = traceId;
        FeedbackUrl = sirsiUrlService.BuildUrl($"/provide-feedback-and-contact/?context=feedback&traceId={TraceId}");
    }

    public string Render()
    {
        var title = "Sorry, there is a problem with the service";
        var feedbackLink = string.Empty;
        if (!string.IsNullOrEmpty(FeedbackUrl))
        {
            feedbackLink = $@"
                <p class=""govuk-body"">
                    If you continue to see this message, <a class=""govuk-link"" href=""{FeedbackUrl}"">contact the support team</a>.
                </p>";
        }
        var traceIdMarkup = string.Empty;
        if (!string.IsNullOrEmpty(TraceId))
        {
            traceIdMarkup = $@"
                <p class=""govuk-body"">
                    <strong>Trace ID:</strong> {TraceId}
                </p>";
        }
        return $@"
            <div class=""govuk-width-container"">
                <main class=""govuk-main-wrapper"" id=""main-content"" role=""main"">
                    <div class=""govuk-grid-row"">
                        <div class=""govuk-grid-column-two-thirds"">
                            <h1 class=""govuk-heading-l"">{title}</h1>
                            <p class=""govuk-body"">Try again later.</p>
                            {feedbackLink}
                            {traceIdMarkup}
                        </div>
                    </div>
                </main>
            </div>";
    }
}