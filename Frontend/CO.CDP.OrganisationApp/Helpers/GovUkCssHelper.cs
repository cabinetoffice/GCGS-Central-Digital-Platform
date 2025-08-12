using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Helpers;

public static class GovUkCssHelper
{
    public static string GetInputWidthCssClass(InputWidthType? inputWidth)
    {
        return inputWidth switch
        {
            InputWidthType.Full => "govuk-!-width-full",
            InputWidthType.TwoThirds => "govuk-!-width-two-thirds",
            InputWidthType.OneHalf => "govuk-!-width-one-half",
            InputWidthType.OneThird => "govuk-!-width-one-third",
            InputWidthType.Width20 => "govuk-input--width-20",
            InputWidthType.Width10 => "govuk-input--width-10",
            InputWidthType.Width5 => "govuk-input--width-5",
            InputWidthType.Width4 => "govuk-input--width-4",
            InputWidthType.Width3 => "govuk-input--width-3",
            InputWidthType.Width2 => "govuk-input--width-2",
            _ => "govuk-!-width-two-thirds"
        };
    }
}
