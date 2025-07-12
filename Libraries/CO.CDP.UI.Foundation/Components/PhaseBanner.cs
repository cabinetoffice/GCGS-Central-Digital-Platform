using CO.CDP.UI.Foundation.Services;
using Microsoft.AspNetCore.Html;

namespace CO.CDP.UI.Foundation.Components;

public static class PhaseBanner
{
    public static HtmlString Render(ISirsiUrlService sirsiUrlService)
    {
        const string feedbackPath = "/contact-us";
        var feedbackUrl = sirsiUrlService.BuildUrl(feedbackPath);

        return new HtmlString($@"
            <div class=""govuk-phase-banner"">
                <p class=""govuk-phase-banner__content"">
                    <strong class=""govuk-tag govuk-phase-banner__content__tag"">
                        Beta
                    </strong>
                    <span class=""govuk-phase-banner__text"">
                        This is a new service – your <a class=""govuk-link"" href=""{feedbackUrl}"">feedback</a> will help us to improve it.
                    </span>
                </p>
            </div>");
    }
}